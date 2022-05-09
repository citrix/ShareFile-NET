#if !PORTABLE && !NETSTANDARD1_3
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.MemoryMappedFiles;
using System.IO;
using System.Net.Http;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Security.Cryptography;
using System.Threading;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class AsyncMemoryMappedFileUploader : AsyncUploaderBase
    {
        private FileChunkHasher hasher;
        private CompletedBytes completedBytes = new CompletedBytes();
        private long initialPosition = 0;

        public AsyncMemoryMappedFileUploader(
            ShareFileClient client,
            UploadSpecificationRequest uploadSpecificationRequest,
            FileStream stream,
            FileUploaderConfig config = null,
            ActiveUploadState activeUploadState = null,
            int? expirationDays = default(int?))
            : base(client, uploadSpecificationRequest, stream, config, expirationDays)
        {
            hasher = new FileChunkHasher(HashProvider);
            if (activeUploadState != null)
            {
                initialPosition = activeUploadState.BytesUploaded;
                UploadSpecification = activeUploadState.UploadSpecification; // what happens if the new UploadSpecRequest disagrees with this?
            }
        }

        public override long LastConsecutiveByteUploaded => completedBytes.CompletedThroughPosition;

        public override async Task PrepareAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (Prepared)
                return;

            UploadSpecification = await CreateUpload(cancellationToken).ConfigureAwait(false);
            await CheckResumeAsync().ConfigureAwait(false);
            Prepared = true;
        }

        private async Task<UploadResponse> FinishUpload(CancellationToken cancellationToken)
        {
            HashProvider.Finalize(new byte[0], 0, 0);
            var finishUri = GetFinishUriForThreadedUploads();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, finishUri);

            var responseMessage = await SendHttpRequest(requestMessage, cancellationToken).ConfigureAwait(false);
            string localHash = HashProvider.GetComputedHashAsString();
            var uploadResponse = await GetUploadResponseAsync(responseMessage, localHash).ConfigureAwait(false);
            if (uploadResponse.Count == 1)
            {
                string serverHash = uploadResponse.Single().Hash;
                if (!string.IsNullOrEmpty(serverHash) && !localHash.Equals(serverHash, StringComparison.OrdinalIgnoreCase))
                    throw new UploadException($"File hash mismatch! Client: {localHash} Server: {serverHash}", Enums.UploadStatusCode.Unknown);
            }

            return uploadResponse;
        }

        private MemoryMappedFile CreateMemoryMappedFile()
        {
            return MemoryMappedFile.CreateFromFile(
                (FileStream)FileStream, // guaranteed from constructor
                mapName: Guid.NewGuid().ToString(), // no collisions with other files
                capacity: 0, // disk file size
                access: MemoryMappedFileAccess.Read,
                inheritability: HandleInheritability.None, // single process
                leaveOpen: true); // caller disposes filestream
        }

        protected override async Task<UploadResponse> InternalUploadAsync(CancellationToken cancellationToken)
        {
            MemoryMappedFile memoryMappedFile = null;
            try
            {
                if (FileStream.Length > 0)
                {
                    memoryMappedFile = CreateMemoryMappedFile();
                    AdjustNumberOfThreadsByFileSize();
                    var partSizeCalc = new PartSizeCalculator(Config.NumberOfThreads, Config.PartConfig);

                    await UploadChunks(
                        memoryMappedFile,
                        partSizeCalc,
                        Config.NumberOfThreads,
                        cancellationToken).ConfigureAwait(false);

                    if (cancellationToken.IsCancellationRequested)
                        throw CancellationException();
                }
                return await FinishUpload(cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                memoryMappedFile?.Dispose();
            }
        }

        private void AdjustNumberOfThreadsByFileSize()
        {
            // copied from scaling - does this still make sense?
            Config.NumberOfThreads = (int)Math.Min(Config.NumberOfThreads, (FileStream.Length / Config.PartConfig.MinFileSizeForMultithreaded) + 1);
        }

        private async Task UploadChunks(MemoryMappedFile memoryMappedFile, PartSizeCalculator partSizeCalc, int workerCount, CancellationToken cancellationToken)
        {
            var hashWorkers = new List<Task<Chunk>>(workerCount);
            var chunkQueue = new Queue<Chunk>(workerCount);
            var uploadWorkers = new List<Task<ChunkUploadTime>>(workerCount);

            int chunkIndex = 0;
            long nextChunkSize = Config.PartConfig.InitialPartSize;
            long positionInFile = initialPosition;
            long bytesRemaining = UploadSpecificationRequest.FileSize - initialPosition;
            try
            {
                while ((bytesRemaining > 0 || hashWorkers.Count > 0 || chunkQueue.Count > 0 || uploadWorkers.Count > 0)
                    && !cancellationToken.IsCancellationRequested)
                {
                    await TryPauseAsync(cancellationToken).ConfigureAwait(false);
                    if (bytesRemaining > 0 && hashWorkers.Count < workerCount)
                    {
                        long chunkSize = Math.Min(nextChunkSize, bytesRemaining);
                        hashWorkers.Add(HashWorker(memoryMappedFile, chunkIndex, positionInFile, chunkSize, cancellationToken));

                        positionInFile += chunkSize;
                        bytesRemaining -= chunkSize;
                        chunkIndex += 1;
                    }
                    else if (chunkQueue.Count > 0 && uploadWorkers.Count < workerCount)
                    {
                        uploadWorkers.Add(UploadWorker(chunkQueue.Dequeue(), progressReporter.ChunkProgressReporter(), cancellationToken));
                    }
                    else if (uploadWorkers.Count > 0 || hashWorkers.Count > 0)
                    {
                        IEnumerable<Task> workersToAwait = uploadWorkers;
                        if (chunkQueue.Count < workerCount)
                            workersToAwait = workersToAwait.Concat(hashWorkers);

                        Task finished = await Task.WhenAny(workersToAwait).ConfigureAwait(false);
                        if (finished is Task<Chunk>)
                        {
                            var finishedHashWorker = (Task<Chunk>)finished;
                            hashWorkers.Remove(finishedHashWorker);
                            chunkQueue.Enqueue(await finishedHashWorker.ConfigureAwait(false));
                        }
                        else
                        {
                            var finishedUploadWorker = (Task<ChunkUploadTime>)finished;
                            uploadWorkers.Remove(finishedUploadWorker);
                            var chunkTime = await finishedUploadWorker.ConfigureAwait(false);
                            nextChunkSize = partSizeCalc.NextPartSize(nextChunkSize, chunkTime.Size, chunkTime.Elapsed);
                        }
                    }
                    else
                    {
                        throw new Exception("Impossible");
                    }
                }
            }
            catch (TaskCanceledException) when (cancellationToken.IsCancellationRequested) { throw CancellationException(); }
            catch (Exception ex) { throw FailureException(ex); }
        }

        private struct Chunk
        {
            public Stream Stream;
            public int Index;
            public long Offset;
            public string Hash;
        }

        private async Task<Chunk> HashWorker(MemoryMappedFile memoryMappedFile, int chunkIndex, long positionInFile, long chunkLength, CancellationToken cancellationToken)
        {
            var stream = memoryMappedFile.CreateViewStream(positionInFile, chunkLength, MemoryMappedFileAccess.Read);
            string hash = await hasher.Append(stream, cancellationToken);
            stream.Seek(0, SeekOrigin.Begin);
            return new Chunk
            {
                Stream = stream,
                Index = chunkIndex,
                Offset = positionInFile,
                Hash = hash,
            };
        }

        private struct ChunkUploadTime
        {
            public long Size;
            public TimeSpan Elapsed;
        }

        private async Task<ChunkUploadTime> UploadWorker(Chunk chunk, ChunkProgressReporter progressReporter, CancellationToken cancellationToken)
        {
            try
            {
                string url = $"{UploadSpecification.ChunkUri}&index={chunk.Index}&byteOffset={chunk.Offset}&hash={chunk.Hash}";
                for (int retries = 0; ; retries++)
                {
                    var timer = Logging.StopwatchFactory.GetStopwatch();
                    timer.Start();
                    try
                    {
                        var requestMessage = ComposeChunkMessage(url, new ProgressStream(chunk.Stream, progressReporter.ReportProgress));
                        var responseMessage = await SendHttpRequest(requestMessage, cancellationToken).ConfigureAwait(false);
                        string responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                        ValidateChunkResponse(responseMessage, responseContent);
                        
                        timer.Stop();
                        lock (completedBytes) { completedBytes.Add(chunk.Offset, chunk.Stream.Length); }
                        var uploadTime = new ChunkUploadTime { Size = chunk.Stream.Length, Elapsed = timer.Elapsed };
                        return uploadTime;
                    }
                    catch (Exception ex)
                    {
                        timer.Stop();
                        progressReporter.ResetProgress();
                        if (retries < Config.PartConfig.PartRetryCount)
                        {
                            chunk.Stream.Seek(0, SeekOrigin.Begin);
                            continue;
                        }
                        if (ex is TaskCanceledException && !cancellationToken.IsCancellationRequested)
                            throw new TimeoutException($"Chunk {chunk.Index} ({chunk.Stream.Length.ToFileSizeString()}) timed out after {timer.Elapsed}");

                        throw;
                    }
                }
            }
            finally
            {
                chunk.Stream.Dispose();
            }
        }

        private HttpRequestMessage ComposeChunkMessage(string url, Stream stream)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            var content = new StreamContent(new NoDisposeStream(stream));
            if (UploadSpecificationRequest.Raw)
            {
                requestMessage.Content = content;
            }
            else
            {
                var multiPartContent = new MultipartFormDataContent();
                content.Headers.Add("Content-Type", "application/octet-stream");
                multiPartContent.Add(content, "Filedata", UploadSpecificationRequest.FileName);
                requestMessage.Content = multiPartContent;
            }
            return requestMessage;
        }

        private async Task<HttpResponseMessage> SendHttpRequest(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            var client = GetHttpClient();
            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.AddDefaultHeaders(Client);

            return await RequestExecutor.SendAsync(
                client,
                requestMessage,
                HttpCompletionOption.ResponseContentRead,
                cancellationToken).ConfigureAwait(false);
        }

        private Exception CancellationException()
        {
            return new UploadException(
                "Upload was cancelled",
                Enums.UploadStatusCode.Cancelled,
                new ActiveUploadState(UploadSpecification, completedBytes.CompletedThroughPosition));
        }

        private Exception FailureException(Exception innerException)
        {
            return new UploadException(
                innerException.Message,
                innerException is UploadException ? (innerException as UploadException).StatusCode : Enums.UploadStatusCode.Unknown,
                new ActiveUploadState(UploadSpecification, completedBytes.CompletedThroughPosition),
                innerException);
        }
    }

    internal class FileChunkHasher
    {
        const long bufferSize = 1024;
        private byte[] zeroBuffer = new byte[0];

        private IMD5HashProvider fileHash;
        private SemaphoreSlim fileHashLock = new SemaphoreSlim(1, 1);

        public FileChunkHasher(IMD5HashProvider fileHash)
        {
            this.fileHash = fileHash;
        }

        public async Task<string> Append(Stream chunk, CancellationToken cancellationToken)
        {
            IMD5HashProvider chunkHash = NewHash();
            byte[] buffer = new byte[bufferSize];
            int bytesRead = 0;

            await fileHashLock.WaitAsync(cancellationToken);
            try
            {
                for (long position = 0; position < chunk.Length; position += bytesRead)
                {
                    bytesRead = await chunk.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    fileHash.Append(buffer, 0, bytesRead);
                    chunkHash.Append(buffer, 0, bytesRead);
                }
            }
            finally { fileHashLock.Release(); }

            chunkHash.Finalize(zeroBuffer, 0, 0);
            return chunkHash.GetComputedHashAsString();
        }

        private static IMD5HashProvider NewHash() => MD5HashProviderFactory.GetHashProvider().CreateHash();
    }
}
#endif
