using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class ScalingPartUploader
    {
        private FilePartConfig partConfig;
        private int concurrentWorkers;
        private Func<HttpRequestMessage, Task> executePartUploadRequest;
        private Action<int, bool> updateProgress;

        public ScalingPartUploader(FilePartConfig partConfig, int concurrentWorkers,
            Func<HttpRequestMessage, Task> executePartUploadRequest,
            Action<int, bool> updateProgress)
        {
            this.partConfig = partConfig;
            this.concurrentWorkers = concurrentWorkers;
            this.executePartUploadRequest = executePartUploadRequest;
            this.updateProgress = updateProgress;
        }

        public Task Upload(IPlatformFile file, IMD5HashProvider hashProvider, string chunkUploadUrl)
        {
            return Task.Factory.StartNew(() =>
            {
                var workers = Dispatch(new FilePartSource(file, hashProvider), chunkUploadUrl).ToArray();
                Task.WaitAll(workers);

                var results = workers.Select(task => task.Result);
                if (!results.All(partUploadResult => partUploadResult.IsSuccess))
                    throw new UploadException("FilePart upload failed", -1, results.Select(result => result.PartUploadException).FirstOrDefault(ex => ex != null));
            });
        }

        private IEnumerable<Task<PartUploadResult>> Dispatch(FilePartSource partSource, string chunkUploadUrl)
        {
            int currentPartSize = partConfig.InitialPartSize; //do not make this a long, needs to be atomic or have a lock

            Func<FilePart, Task> attemptPartUpload = part =>
            {
                var timer = ShareFile.Api.Client.Logging.Stopwatch.StartNew();
                return UploadPart(chunkUploadUrl, part).ContinueWith(workerTask =>
                {
                    timer.Stop();
                    workerTask.Rethrow();
                    int partSizeIncrement = CalculatePartSizeIncrement(part.Bytes.Length, TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds));
                    //this increment isn't thread-safe, but nothing horrible should happen if it gets clobbered
                    currentPartSize = (currentPartSize + partSizeIncrement).Bound(partConfig.MaxPartSize, partConfig.MinPartSize);
                });
            };

            var workers = new AsyncSemaphore(concurrentWorkers);
            bool giveUp = false;
            while (!giveUp && partSource.HasMore)
            {
                workers.WaitAsync().Wait();
                var part = partSource.GetNextPart(currentPartSize);
                if (part == null || giveUp)
                {
                    yield return TaskFromResult(PartUploadResult.Error);
                    break; //stream is busted
                }

                var task = AttemptPartUploadWithRetry(attemptPartUpload, part, partConfig.PartRetryCount)
                    .ContinueWith(partUploadTask =>
                    {
                        var partResult = partUploadTask.Result;
                        if (!partResult.IsSuccess)
                            giveUp = true;
                        workers.Release();
                        return partResult;
                    });
                yield return task;
            }
            yield break;
        }

        private int CalculatePartSizeIncrement(long partSize, TimeSpan elapsedTime)
        {
            //connection speed values are bytes/second
            double estimatedConnectionSpeed = partSize / elapsedTime.TotalSeconds;
            double targetPartSize = estimatedConnectionSpeed * partConfig.TargetPartUploadTime.TotalSeconds;
            double partSizeDelta = targetPartSize - partSize;

            //initial batch of workers will all calculate ~same delta; penalize for >1
            partSizeDelta = partSizeDelta / this.concurrentWorkers;

            //bound the delta to a multiple of partsize in case of extreme result
            partSizeDelta = partSizeDelta.Bound(
                partSize * (partConfig.MaxPartSizeIncreaseFactor - 1.0),
                partSize * (-1.0 * (partConfig.MaxPartSizeDecreaseFactor - 1.0) / partConfig.MaxPartSizeDecreaseFactor));

            return Convert.ToInt32(partSizeDelta);
        }

        private HttpRequestMessage ComposePartUpload(string chunkUploadUrl, FilePart part)
        {
            string uploadUri = part.GetComposedUploadUrl(chunkUploadUrl);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uploadUri) { Content = new ByteArrayContent(part.Bytes) };
            return requestMessage;
        }

        private Task UploadPart(string chunkUploadUrl, FilePart part)
        {
            return executePartUploadRequest(ComposePartUpload(chunkUploadUrl, part))
                .ContinueWith(task =>
                {
                    task.Rethrow();
                    updateProgress(part.Bytes.Length, part.IsLastPart);
                });
        }

        //exception boundary: chunk upload exceptions should be propagated to here but no farther
        private Task<PartUploadResult> AttemptPartUploadWithRetry(Func<FilePart, Task> attemptUpload, FilePart part, int retryCount)
        {
            if (retryCount < 0)
                return TaskFromResult(PartUploadResult.Error);

            return attemptUpload(part).ContinueWith(uploadTask =>
            {
                if(uploadTask.Exception != null)
                {
                    if (retryCount < 0)
                        return PartUploadResult.Exception(uploadTask.Exception.Unwrap());
                    else
                        return AttemptPartUploadWithRetry(attemptUpload, part, retryCount - 1).Result;
                }
                else
                {
                    return PartUploadResult.Success;
                }
            });
        }        

        //in .NET 4.5, Task.FromResult
        private Task<T> TaskFromResult<T>(T value)
        {
            var tcs = new TaskCompletionSource<T>();
            tcs.SetResult(value);
            return tcs.Task;
        }


        internal class PartUploadResult
        {
            public bool IsSuccess { get; set; }
            public Exception PartUploadException { get; set; }

            public static PartUploadResult Success = new PartUploadResult { IsSuccess = true };
            public static PartUploadResult Error = new PartUploadResult { IsSuccess = false };
            public static PartUploadResult Exception(Exception ex) { return new PartUploadResult { IsSuccess = false, PartUploadException = ex }; }
        }

        internal class FilePartSource
        {
            public bool HasMore { get; private set; }

            private IMD5HashProvider fileHash;
            private Stream stream; //the calling application instantiates IPlatformFile which controls the life of this stream
            private long fileLength;
            private long streamPosition;
            private int partCount;

            public FilePartSource(IPlatformFile file, IMD5HashProvider hashProvider)
            {
                fileHash = hashProvider;
                stream = file.OpenRead();
                fileLength = file.Length;
                streamPosition = 0;
                partCount = 0;
                HasMore = true;
            }

            public FilePart GetNextPart(long requestedPartSize)
            {
                try
                {
                    int partSize = Convert.ToInt32(Math.Min(requestedPartSize, fileLength - streamPosition));
                    byte[] content = new byte[partSize];
                    stream.Read(content, 0, partSize);

                    bool isLast = streamPosition + partSize >= fileLength;
                    string hash = MD5HashProviderFactory.GetHashProvider().CreateHash().ComputeHash(content);
                    var part = new FilePart { Bytes = content, Hash = hash, Index = partCount, Offset = streamPosition, IsLastPart = isLast };

                    streamPosition += partSize;
                    partCount += 1;
                    if (isLast)
                    {
                        HasMore = false;
                        fileHash.Finalize(content, 0, content.Length);
                    }
                    else
                        fileHash.Append(content, 0, content.Length);

                    return part;
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
