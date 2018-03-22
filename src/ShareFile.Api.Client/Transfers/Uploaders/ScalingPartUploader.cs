using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Logging;
using System.Buffers;
using ShareFile.Api.Client.Transfers.Uploaders.Buffers;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class ScalingPartUploader
    {
        private readonly FilePartConfig partConfig;
        private int concurrentWorkers;
        private readonly Func<HttpRequestMessage, CancellationToken, Task> executePartUploadRequest;
        private readonly Action<long> updateProgress;
        private readonly LoggingProvider logger;
        private readonly CompletedBytes completedBytes = new CompletedBytes();
        private bool started;
        private bool raw;

        public ScalingPartUploader(
            FilePartConfig partConfig,
            int concurrentWorkers,
            Func<HttpRequestMessage, CancellationToken, Task> executePartUploadRequest,
            Action<long> updateProgress,
            LoggingProvider logger)
        {
            this.partConfig = partConfig;
            this.executePartUploadRequest = executePartUploadRequest;
            this.updateProgress = updateProgress;
            this.logger = logger;
            this.NumberOfThreads = concurrentWorkers;
        }

        public int NumberOfThreads
        {
            get
            {
                return concurrentWorkers;
            }
            set
            {
                if (started)
                {
                    logger.Info("Cannot change NumberOfThreads after the upload has started.");
                    return;
                }
                concurrentWorkers = value >= 1 ? value : 1;
            }
        }

        /// <summary>
        /// Gets the last byte that was uploaded with no missing bytes before it.
        /// <para>If bytes 1,2,3,6,7 where uploaded, then this will return 3.</para>
        /// </summary>
        public long LastConsecutiveByteUploaded
        {
            get
            {
                return completedBytes.CompletedThroughPosition;
            }
        }

        public UploadSpecification UploadSpecification { get; set; }

        public async Task Upload(
            Stream fileStream,
            string fileName,
            IMD5HashProvider hashProvider,
            string chunkUploadUrl,
            bool raw,
            long offset = 0,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            this.raw = raw;
            // We block this after setting 'started', so make sure this statement is first
            NumberOfThreads = (int)Math.Min(NumberOfThreads, (fileStream.Length / partConfig.MinFileSizeForMultithreaded) + 1);
            started = true;
            if (offset != 0)
            {
                updateProgress(offset);
                completedBytes.Add(0, offset);
            }
            try
            {
                var filePartSource = new FilePartSource(fileStream, hashProvider, partConfig.BufferAllocator, offset);
                var workers = await Dispatch(filePartSource, chunkUploadUrl, fileName, cancellationToken).ConfigureAwait(false);
                await Task.WhenAll(workers).ConfigureAwait(false);
                logger.Info("[Scaling Uploader] All upload parts succeeded");
            }
            catch(Exception innerException)
            {
                logger.Info("[Scaling Uploader] Upload failed. Bytes uploaded: " + LastConsecutiveByteUploaded);
                var uploadException = innerException as UploadException;
                UploadStatusCode statusCode = uploadException == null ? UploadStatusCode.Unknown : uploadException.StatusCode;
                throw new UploadException(
                    "FilePart upload failed",
                    statusCode,
                    new ActiveUploadState(UploadSpecification, LastConsecutiveByteUploaded),
                    innerException);
            }
        }

        private async Task<List<Task>> Dispatch(FilePartSource partSource, string chunkUploadUrl, string fileName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var incrementLock = new object();
            long currentPartSize = partConfig.InitialPartSize;
            var partSizeCalc = new PartSizeCalculator(concurrentWorkers, partConfig);

            Func<FilePart, Task> attemptPartUpload = async part =>
            {
                IStopwatch timer = Stopwatch.StartNew();
                try
                {
                    await UploadPart(chunkUploadUrl, part, fileName, cancellationToken).ConfigureAwait(false);
                }
                finally
                {
                    timer.Stop();
                }
                lock (incrementLock)
                {
                    currentPartSize = partSizeCalc.NextPartSize(currentPartSize, part.Bytes.Length, timer.Elapsed);
                }
            };

            var workerTasks = new List<Task>();
            try
            {
                var activeWorkers = new AsyncSemaphore(concurrentWorkers);
                bool giveUp = false;
                while (!giveUp && partSource.HasMore)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new UploadException(
                            "Upload was cancelled",
                            UploadStatusCode.Cancelled,
                            new ActiveUploadState(UploadSpecification, LastConsecutiveByteUploaded));
                    }
                    await activeWorkers.WaitAsync().ConfigureAwait(false);
                    if (giveUp)
                        return workerTasks;

                    var part = await partSource.GetNextPart(Interlocked.Read(ref currentPartSize)).ConfigureAwait(false);
                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            await AttemptPartUploadWithRetry(attemptPartUpload, part, partConfig.PartRetryCount).ConfigureAwait(false);
                        }
                        catch
                        {
                            giveUp = true;
                            throw;
                        }
                        finally
                        {
                            activeWorkers.Release();
                            part.Bytes.Dispose();
                        }
                    });
                    workerTasks.Add(task);
                }
                return workerTasks;
            }
            catch
            {
                ObserveExceptions(workerTasks);
                throw;
            }
        }

        private static void ObserveExceptions(IList<Task> tasks)
        {
            Task.Run(async () =>
            {
                try
                {
                    await Task.WhenAll(tasks).ConfigureAwait(false);
                }
                catch { }
            }).ConfigureAwait(false);
        }

        private HttpRequestMessage ComposePartUpload(string chunkUploadUrl, FilePart part, string filename)
        {
            string uploadUri = part.GetComposedUploadUrl(chunkUploadUrl);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uploadUri);
            var content = new StreamContentWithProgress(part.Bytes.GetStream(), updateProgress);
            requestMessage.Content = content;

            if (!raw)
            {
                var multiPartContent = new MultipartFormDataContent();                
                content.Headers.Add("Content-Type", "application/octet-stream");                                
                multiPartContent.Add(content, "Filedata", filename);                
                requestMessage.Content = multiPartContent;
            }            

            return requestMessage;
        }
        
        private async Task UploadPart(string chunkUploadUrl, FilePart part, string filename, CancellationToken cancellationToken)
        {
            try
            {
                HttpRequestMessage partRequest = ComposePartUpload(chunkUploadUrl, part, filename);
                await executePartUploadRequest(partRequest, cancellationToken).ConfigureAwait(false);
            }
            catch(OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // If the task was cancelled but a cancel wasn't requested, then consider it a timeout
                throw new TimeoutException();
            }
        }

        private async Task AttemptPartUploadWithRetry(Func<FilePart, Task> attemptUpload, FilePart part, int retryCount)
        {
            for(int attempts = 0; attempts <= retryCount; attempts++)
            {
                try
                {
                    await attemptUpload(part).ConfigureAwait(false);
                    lock (completedBytes)
                    {
                        completedBytes.Add(part.Offset, part.Length);
                    }
                    return;
                }
                catch (UploadException uploadException) when (uploadException.IsInvalidUploadId)
                {
                    throw;
                }
                catch (Exception)
                {
                    if (attempts >= retryCount)
                        throw;
                }
            }
            throw new UploadException("Shouldn't get here", UploadStatusCode.Unknown);            
        }

        internal class FilePartSource
        {
            public bool HasMore { get; private set; }

            private readonly IMD5HashProvider fileHash;
            private readonly Stream fileStream;
            private readonly IBufferAllocator bufferAllocator;
            private readonly long fileLength;

            private long streamPosition;
            private int partCount;

            public FilePartSource(Stream fileStream, IMD5HashProvider hashProvider, IBufferAllocator bufferAllocator, long offset)
            {
                fileHash = hashProvider;
                this.fileStream = fileStream;
                this.bufferAllocator = bufferAllocator;
                fileLength = fileStream.Length;
                streamPosition = offset;
                this.fileStream.Seek(streamPosition, SeekOrigin.Begin);
                partCount = 0;
                HasMore = true;
            }

            public async Task<FilePart> GetNextPart(long requestedPartSize)
            {
                try
                {
                    int partSize = Convert.ToInt32(Math.Min(requestedPartSize, fileLength - streamPosition));
                    ReadFileResult partContent = await ReadFile(partSize).ConfigureAwait(false);
                    bool isLast = streamPosition + partSize >= fileLength;
                    var part = new FilePart(partContent.Content, partCount, streamPosition, partSize, partContent.Hash, isLast);
                    if (isLast)
                    {
                        HasMore = false;
                        fileHash.Finalize(ArrayPool<byte>.Shared.Rent(0), 0, 0);
                    }
                    else
                    {
                        streamPosition += partSize;
                        partCount += 1;
                    }
                    return part;
                }
                catch
                {
                    HasMore = false;
                    throw;
                }
            }

            private struct ReadFileResult
            {
                public IBuffer Content;
                public string Hash;
            }

            private async Task<ReadFileResult> ReadFile(int length)
            {
                IBuffer content = bufferAllocator.Allocate(length);
                Stream dest = content.GetStream();
                IMD5HashProvider chunkHash = MD5HashProviderFactory.GetHashProvider().CreateHash();
                byte[] b = ArrayPool<byte>.Shared.Rent(Configuration.BufferSize);
                try
                {
                    int read = 0;
                    int toRead = length;
                    do
                    {
                        read = await fileStream.ReadAsync(b, offset: 0, count: Math.Min(b.Length, toRead)).ConfigureAwait(false);
                        toRead -= read;

                        await dest.WriteAsync(b, offset: 0, count: read).ConfigureAwait(false);
                        chunkHash.Append(b, offset: 0, size: read);
                        fileHash.Append(b, offset: 0, size: read);
                    } while (read > 0 && toRead > 0);
                    if (toRead > 0)
                        throw new Exception($"Expected to read {length} bytes, actual read {length - toRead} bytes");

                    chunkHash.Finalize(ArrayPool<byte>.Shared.Rent(0), 0, 0);
                    return new ReadFileResult { Content = content, Hash = chunkHash.GetComputedHashAsString() };
                }
                catch
                {
                    content.Dispose();
                    throw;
                }
                finally
                {
                    ArrayPool<byte>.Shared.Return(b);
                    dest.Dispose();
                }
            }
        }
    }
}