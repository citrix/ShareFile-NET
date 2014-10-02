using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class ScalingChunkUploader
    {
        private FileChunkConfig chunkConfig;
        private int concurrentWorkers;
        private Func<HttpRequestMessage, Task> executeRequest;
        private Action<int, bool> updateProgress;

        public ScalingChunkUploader(FileChunkConfig chunkConfig, int concurrentWorkers,
            Func<HttpRequestMessage, Task> executeRequest,
            Action<int, bool> updateProgress)
        {
            this.chunkConfig = chunkConfig;
            this.concurrentWorkers = concurrentWorkers;
            this.executeRequest = executeRequest;
            this.updateProgress = updateProgress;
        }

        public Task Upload(IPlatformFile file, IMD5HashProvider hashProvider, string chunkUploadUrl)
        {
            return Task.Factory.StartNew(() =>
            {
                var workers = Dispatch(new FileChunkSource(file, hashProvider), chunkUploadUrl).ToArray();
                Task.WaitAll(workers);

                var results = workers.Select(task => task.Result);
                if (!results.All(chunkResult => chunkResult.IsSuccess))
                    throw new UploadException("Chunk upload failed", -1, results.Select(result => result.Exception).FirstOrDefault(ex => ex != null));
            });
        }

        private IEnumerable<Task<ChunkUploadResult>> Dispatch(FileChunkSource chunkSource, string chunkUploadUrl)
        {
            int currentChunkSize = chunkConfig.InitialChunkSize; //do not make this a long, needs to be atomic or have a lock

            Func<FileChunk, Task> attemptChunkUpload = workerChunk =>
            {
                var timer = Stopwatch.StartNew();
                return UploadChunk(chunkUploadUrl, workerChunk).ContinueWith(workerTask =>
                {
                    timer.Stop();
                    int chunkIncrement = CalculateChunkIncrement(workerChunk.Content.Length, timer.Elapsed);
                    //this increment isn't thread-safe, but nothing horrible should happen if it gets clobbered
                    currentChunkSize = (currentChunkSize + chunkIncrement).Bound(chunkConfig.MaxChunkSize, chunkConfig.MinChunkSize);
                });
            };

            var workers = new SemaphoreSlim(concurrentWorkers);
            bool giveUp = false;
            while (!giveUp && chunkSource.HasMore)
            {
                workers.Wait();
                var chunk = chunkSource.GetNextChunk(currentChunkSize);
                if (chunk == null || giveUp)
                    break; //stream is busted

                var task = AttemptChunkUploadWithRetry(attemptChunkUpload, (FileChunk)chunk, chunkConfig.ChunkRetryCount)
                    .ContinueWith(chunkUploadTask =>
                    {
                        var chunkResult = chunkUploadTask.Result;
                        if (!chunkResult.IsSuccess)
                            giveUp = true;
                        workers.Release();
                        return chunkResult;
                    });
                yield return task;
            }
            yield break;
        }

        private int CalculateChunkIncrement(long chunkSize, TimeSpan elapsedTime)
        {
            //connection speed values are bytes/second
            double estimatedConnectionSpeed = chunkSize / elapsedTime.TotalSeconds;
            double targetChunkSize = estimatedConnectionSpeed * chunkConfig.TargetChunkUploadTime.TotalSeconds;
            double chunkSizeDelta = targetChunkSize - chunkSize;

            //initial batch of workers will all calculate ~same delta; penalize for >1
            chunkSizeDelta = chunkSizeDelta / this.concurrentWorkers;

            //bound the delta to a multiple of chunksize in case of extreme result
            chunkSizeDelta = chunkSizeDelta.Bound(
                chunkSize * (chunkConfig.MaxChunkIncreaseFactor - 1.0),
                chunkSize * (-1.0 * (chunkConfig.MaxChunkDecreaseFactor - 1.0) / chunkConfig.MaxChunkDecreaseFactor));

            return Convert.ToInt32(chunkSizeDelta);
        }

        private HttpRequestMessage ComposeChunkUpload(string chunkUploadUrl, FileChunk chunk)
        {
            string uploadUri = string.Format("{0}&index={1}&byteOffset={2}&hash={3}", chunkUploadUrl, chunk.Index, chunk.Offset, chunk.Hash);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uploadUri) { Content = new ByteArrayContent(chunk.Content) };
            return requestMessage;
        }

        private Task UploadChunk(string chunkUploadUrl, FileChunk chunk)
        {
            return executeRequest(ComposeChunkUpload(chunkUploadUrl, chunk))
                .ContinueWith(task => updateProgress(chunk.Content.Length, chunk.IsLast));
        }

        private Task<ChunkUploadResult> AttemptChunkUploadWithRetry(Func<FileChunk, Task> attemptUpload, FileChunk chunk, int retryCount)
        {
            if (retryCount < 0)
                return TaskFromResult(ChunkUploadResult.Error(null));

            try
            {
                return attemptUpload(chunk).ContinueWith(uploadTask => ChunkUploadResult.Success);
            }
            catch (Exception ex)
            {
                //TODO: scope down to network errors and timeouts
                if (retryCount > 0)
                    return AttemptChunkUploadWithRetry(attemptUpload, chunk, retryCount - 1);
                else
                    return TaskFromResult(ChunkUploadResult.Error(ex));
            }
        }

        //in .NET 4.5, Task.FromResult
        private Task<T> TaskFromResult<T>(T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }


        internal class ChunkUploadResult
        {
            public bool IsSuccess { get; set; }
            public Exception Exception { get; set; }

            public static ChunkUploadResult Success = new ChunkUploadResult { IsSuccess = true };
            public static ChunkUploadResult Error(Exception ex) { return new ChunkUploadResult { IsSuccess = false, Exception = ex }; }
        }

        internal class FileChunk
        {
            public byte[] Content { get; set; }
            public string Hash { get; set; }
            public long Offset { get; set; }
            public int Index { get; set; }
            public bool IsLast { get; set; }
        }

        internal class FileChunkSource
        {
            public bool HasMore { get; private set; }

            private IMD5HashProvider fileHash;
            private Stream stream; //the calling application instantiates IPlatformFile which controls the life of this stream
            private long fileLength;
            private long streamPosition;
            private int chunkCount;

            public FileChunkSource(IPlatformFile file, IMD5HashProvider hashProvider)
            {
                fileHash = hashProvider;
                stream = file.OpenRead();
                fileLength = file.Length;
                streamPosition = 0;
                chunkCount = 0;
                HasMore = true;
            }

            public FileChunk GetNextChunk(long requestedChunkSize)
            {
                try
                {
                    int chunkSize = Convert.ToInt32(Math.Min(requestedChunkSize, fileLength - streamPosition));
                    byte[] content = new byte[chunkSize];
                    stream.Read(content, 0, chunkSize);

                    bool isLast = streamPosition + chunkSize >= fileLength;
                    string hash = MD5HashProviderFactory.GetHashProvider().CreateHash().ComputeHash(content);
                    var chunk = new FileChunk { Content = content, Hash = hash, Index = chunkCount, Offset = streamPosition, IsLast = isLast };

                    streamPosition += chunkSize;
                    chunkCount += 1;
                    if (isLast)
                    {
                        HasMore = false;
                        fileHash.Finalize(content, 0, content.Length);
                    }
                    else
                        fileHash.Append(content, 0, content.Length);

                    return chunk;
                }
                catch
                {
                    //improvement: return object to propagate this exception
                    HasMore = false;
                    return null;
                }
            }
        }
    }
}
