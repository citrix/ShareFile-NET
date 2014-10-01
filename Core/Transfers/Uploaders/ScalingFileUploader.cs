#if !Async
using ShareFile.Api.Client.Exceptions;
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
    public class ScalingFileUploader : SyncUploaderBase
    {
        private TimeSpan targetChunkUploadTime;
        private int maxChunkSize;
        private int minChunkSize;

        public ScalingFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            targetChunkUploadTime = TimeSpan.FromSeconds(15);
            maxChunkSize = FileUploaderConfig.DefaultPartSize;
            minChunkSize = 5 * 1024;
        }

        public override UploadResponse Upload(Dictionary<string, object> transferMetadata = null)
        {
            SetUploadSpecification();

            var workers = Dispatch(new FileChunkSource(File, HashProvider)).ToArray();
            Task.WaitAll(workers);

            var results = workers.Select(task => task.Result);
            if (!results.All(chunkResult => chunkResult.IsSuccess))
                throw new UploadException("Chunk upload failed", -1, results.Select(result => result.Exception).FirstOrDefault(ex => ex != null));

            return FinishUpload();
        }
        
        private UploadResponse FinishUpload()
        {
            var finishUri = GetFinishUriForThreadedUploads();
            var client = GetHttpClient();

            var message = new HttpRequestMessage(HttpMethod.Get, finishUri);
            message.Headers.Add("Accept", "application/json");

            var response = client.SendAsync(message).WaitForTask();

            return GetUploadResponse(response);
        }

        private void UploadChunk(FileChunk chunk)
        {
            string uploadUri = string.Format("{0}&index={1}&byteOffset={2}&hash={3}", UploadSpecification.ChunkUri.AbsoluteUri, chunk.Index, chunk.Offset, chunk.Hash);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uploadUri) { Content = new ByteArrayContent(chunk.Content) };

            using(var responseMessage = GetHttpClient().SendAsync(requestMessage).WaitForTask())
            {
                DeserializeShareFileApiResponse<string>(responseMessage);
            }

            UpdateProgress(chunk);
        }

        private void UpdateProgress(FileChunk chunk)
        {
            Progress.BytesTransferred += chunk.Content.Length;
            Progress.BytesRemaining -= chunk.Content.Length;
            Progress.Complete = chunk.IsLast;
            NotifyProgress(Progress);
        }
         
        private T Bound<T>(T value, T upperBound, T lowerBound) where T : IComparable
        {
            if (value.CompareTo(upperBound) == 1)
                return upperBound;
            else if (value.CompareTo(lowerBound) == -1)
                return lowerBound;
            else
                return value;
        }

        private int CalculateChunkIncrement(long chunkSize, TimeSpan targetTime, TimeSpan elapsedTime, int concurrentWorkers)
        {
            //connection speed values are bytes/second
            double estimatedConnectionSpeed = chunkSize / elapsedTime.TotalSeconds;
            double targetChunkSize = estimatedConnectionSpeed * targetTime.TotalSeconds;
            double chunkSizeDelta = targetChunkSize - chunkSize;

            //initial batch of workers will all calculate ~same delta; penalize for >1
            chunkSizeDelta = chunkSizeDelta / concurrentWorkers; 
            
            //bound the delta in case of extreme result
            chunkSizeDelta = Bound(chunkSizeDelta, chunkSize * 3.0, chunkSize * -0.75);

            return Convert.ToInt32(chunkSizeDelta);
        }
        
        private IEnumerable<Task<ChunkUploadResult>> Dispatch(FileChunkSource chunkSource)
        {
            int currentChunkSize = Config.ScalingPartSize; //do not make this a long, needs to be atomic or have a lock

            Action<FileChunk> attemptChunkUpload = workerChunk =>
                {
                    var timer = Stopwatch.StartNew();
                    UploadChunk(workerChunk);
                    timer.Stop();
                    int chunkIncrement = CalculateChunkIncrement(workerChunk.Content.Length, targetChunkUploadTime, timer.Elapsed, Config.NumberOfThreads);
                    //this increment isn't thread-safe, but nothing horrible should happen if it gets clobbered
                    currentChunkSize = Bound(currentChunkSize + chunkIncrement, maxChunkSize, minChunkSize);
                };

            var workers = new SemaphoreSlim(Config.NumberOfThreads);
            bool giveUp = false;
            while (!giveUp && chunkSource.HasMore)
            {
                workers.Wait(); 
                var chunk = chunkSource.GetNextChunk(currentChunkSize);
                if (chunk == null || giveUp)
                    break; //stream is busted

                var task = Task.Factory.StartNew(workerChunk =>
                    {
                        if (giveUp)
                            return ChunkUploadResult.Error(null);
                        var chunkResult = AttemptChunkUploadWithRetry(attemptChunkUpload, (FileChunk)workerChunk, 1);
                        if (!chunkResult.IsSuccess)
                            giveUp = true;
                        workers.Release();
                        return chunkResult;
                    }, chunk);
                yield return task;
            }
            yield break;
        }

        private ChunkUploadResult AttemptChunkUploadWithRetry(Action<FileChunk> attemptUpload, FileChunk chunk, int retryCount)
        {
            if (retryCount < 0)
                return ChunkUploadResult.Error(null);

            try
            {
                attemptUpload(chunk);
                return ChunkUploadResult.Success;
            }
            catch(Exception ex)
            {
                //TODO: scope down to network errors and timeouts
                if (retryCount > 0)
                    return AttemptChunkUploadWithRetry(attemptUpload, chunk, retryCount - 1);
                else
                    return ChunkUploadResult.Error(ex);
            }
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }

        private UploadSpecification SetUploadSpecification()
        {
            if(UploadSpecification == null)
            {
                UploadSpecification = CreateUploadSpecificationQuery(UploadSpecificationRequest).Execute();
            }
            return UploadSpecification;
        }
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
#endif