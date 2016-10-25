using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Logging;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class ScalingPartUploader
    {
        private readonly FilePartConfig partConfig;
        private int concurrentWorkers;
        private readonly Func<HttpRequestMessage, Task> executePartUploadRequest;
        private readonly Action<long> updateProgress;
        private readonly LoggingProvider logger;
        private readonly CompletedBytes completedBytes = new CompletedBytes();
        private bool started;
        private bool raw;

        public ScalingPartUploader(
            FilePartConfig partConfig,
            int concurrentWorkers,
            Func<HttpRequestMessage, Task> executePartUploadRequest,
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

        public Task Upload(
            IPlatformFile file,
            IMD5HashProvider hashProvider,
            string chunkUploadUrl,
            bool raw,
            long offset = 0,
            CancellationToken? cancellationToken = null)
        {
            this.raw = raw;
            // We block this after setting 'started', so make sure this statement is first
            NumberOfThreads = (int)Math.Min(NumberOfThreads, (file.Length / partConfig.MinFileSizeForMultithreaded) + 1);
            started = true;
            return Task.Factory.StartNew(() =>
            {
                if (offset != 0)
                {
                    updateProgress(offset);
                    completedBytes.Add(0, offset);
                }
                var workers = Dispatch(new FilePartSource(file, hashProvider, offset), chunkUploadUrl, file.Name, cancellationToken).ToArray();
                Task.WaitAll(workers);

                var results = workers.Select(task => task.Result);
                if (!results.All(partUploadResult => partUploadResult.IsSuccess))
                {
                    logger.Info("[Scaling Uploader] Upload failed. Bytes uploaded: " + LastConsecutiveByteUploaded);
                    var innerException = results.Select(result => result.PartUploadException).FirstOrDefault(ex => ex != null);
                    var uploadException = innerException as UploadException;
                    UploadStatusCode statusCode = uploadException == null ? UploadStatusCode.Unknown : uploadException.StatusCode;
                    throw new UploadException(
                        "FilePart upload failed",
                        statusCode,
                        new ActiveUploadState(UploadSpecification, LastConsecutiveByteUploaded),
                        innerException);
                }
                logger.Info("[Scaling Uploader] All upload parts succeeded");
            });
        }

        private IEnumerable<Task<PartUploadResult>> Dispatch(FilePartSource partSource, string chunkUploadUrl, string fileName, CancellationToken? cancellationToken = null)
        {
            var incrementLock = new object();
            int currentPartSize = partConfig.InitialPartSize; //do not make this a long, needs to be atomic or have a lock

            Func<FilePart, Task> attemptPartUpload = part =>
            {
                var timer = ShareFile.Api.Client.Logging.Stopwatch.StartNew();
                return UploadPart(chunkUploadUrl, part, fileName, cancellationToken).ContinueWith(workerTask =>
                {
                    timer.Stop();
                    workerTask.Rethrow();
                    int partSizeIncrement = CalculatePartSizeIncrement(part.Bytes.Length, TimeSpan.FromMilliseconds(timer.ElapsedMilliseconds));

                    lock (incrementLock)
                    {
                        currentPartSize = (currentPartSize + partSizeIncrement).Bound(partConfig.MaxPartSize, partConfig.MinPartSize);
                    }
                });
            };

            var workers = new AsyncSemaphore(concurrentWorkers);
            bool giveUp = false;
            while (!giveUp && partSource.HasMore)
            {
                if (cancellationToken.GetValueOrDefault().IsCancellationRequested)
                {
                    throw new UploadException(
                        "Upload was cancelled",
                        UploadStatusCode.Cancelled,
                        new ActiveUploadState(UploadSpecification, LastConsecutiveByteUploaded));
                }
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
                        {
                            giveUp = true;
                        }
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

        private HttpRequestMessage ComposePartUpload(string chunkUploadUrl, FilePart part,string filename)
        {
            string uploadUri = part.GetComposedUploadUrl(chunkUploadUrl);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uploadUri);
            var content = new ByteArrayContentWithProgress(part.Bytes, bytesWritten => updateProgress(bytesWritten));
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

        private Task UploadPart(string chunkUploadUrl, FilePart part, string filename, CancellationToken? cancellationToken)
        {
            return executePartUploadRequest(ComposePartUpload(chunkUploadUrl, part,filename))
                .ContinueWith(
                    task =>
                        {
                            task.Rethrow();
                            // If the task was cancelled but a cancel wasn't requested, then consider it a timeout
                            if (task.IsCanceled && !cancellationToken.GetValueOrDefault().IsCancellationRequested)
                            {
                                throw new TimeoutException();
                            }
                        });
        }

        //exception boundary: chunk upload exceptions should be propagated to here but no farther
        private Task<PartUploadResult> AttemptPartUploadWithRetry(Func<FilePart, Task> attemptUpload, FilePart part, int retryCount)
        {
            if (retryCount < 0)
                return TaskFromResult(PartUploadResult.Error);

            return attemptUpload(part).ContinueWith(uploadTask =>
            {
                if (uploadTask.Exception != null)
                {
                    var uploadException = uploadTask.Exception.Unwrap() as UploadException;
                    //if (retryCount > 0 && !(uploadException?.IsInvalidUploadId).GetValueOrDefault())
                    if (retryCount > 0 && !(uploadException != null && uploadException.IsInvalidUploadId))
                        return AttemptPartUploadWithRetry(attemptUpload, part, retryCount - 1).Result;
                    else
                        return PartUploadResult.Exception(uploadTask.Exception.Unwrap());
                }
                else
                {
                    lock (completedBytes)
                    {
                        completedBytes.Add(part.Offset, part.Length);
                    }
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

            public FilePartSource(IPlatformFile file, IMD5HashProvider hashProvider, long offset = 0)
            {
                fileHash = hashProvider;
                stream = file.OpenRead();
                fileLength = file.Length;
                streamPosition = offset;
                stream.Seek(streamPosition, SeekOrigin.Begin);
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
                    var part = new FilePart(content, partCount, streamPosition, partSize, hash, isLast);

                    streamPosition += partSize;
                    partCount += 1;
                    if (isLast)
                    {
                        HasMore = false;
                        fileHash.Finalize(content, 0, content.Length);
                    }
                    else
                    {
                        fileHash.Append(content, 0, content.Length);
                    }

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