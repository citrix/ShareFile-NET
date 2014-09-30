﻿#if !Async
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShareFile.Api.Client.Transfers.Uploaders;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Extensions.Tasks;


namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class ScalingFileUploader : SyncUploaderBase
    {
        private TimeSpan targetChunkUploadTime;
        private int maxChunkSize;

        public ScalingFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            targetChunkUploadTime = TimeSpan.FromSeconds(30);
            maxChunkSize = 4 * 1024 * 1024;
        }

        public override UploadResponse Upload(Dictionary<string, object> transferMetadata = null)
        {
            SetUploadSpecification();

            var workers = Dispatch(new FileChunkSource(File)).ToArray();
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
            Progress.BytesTransferred += chunk.Content.Length;
            NotifyProgress(Progress);
        }

        private int CalculateChunkIncrement(long chunkSize, TimeSpan targetTime, TimeSpan elapsedTime, int concurrentWorkers)
        {
            //TODO: logic!
            return 0;
        }

        private IEnumerable<Task<ChunkUploadResult>> Dispatch(FileChunkSource chunkSource)
        {
            int currentChunkSize = Config.PartSize; //do not make this a long, needs to be atomic or have a lock

            Func<FileChunk, ChunkUploadResult> attemptChunkUpload = workerChunk =>
                {
                    var started = DateTime.Now;
                    UploadChunk(workerChunk);
                    var elapsed = DateTime.Now - started; //is there a better way to calculate this? stopwatch?
                    int chunkIncrement = CalculateChunkIncrement(workerChunk.Content.Length, targetChunkUploadTime, elapsed, Config.NumberOfThreads);
                    //this increment isn't thread-safe, but nothing horrible should happen if it gets clobbered
                    currentChunkSize = Math.Max(currentChunkSize + chunkIncrement, maxChunkSize); 
                    return ChunkUploadResult.Success;
                };

            var workers = new SemaphoreSlim(Config.NumberOfThreads);
            bool giveUp = false;
            while (!giveUp && chunkSource.HasMore)
            {
                workers.Wait(); //deadlock timeout here?
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

        private ChunkUploadResult AttemptChunkUploadWithRetry(Func<FileChunk, ChunkUploadResult> attemptUpload, FileChunk chunk, int retryCount)
        {
            if (retryCount < 0)
                return ChunkUploadResult.Error(null);

            try
            {
                var result = attemptUpload(chunk);
                return result;
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
        public long Offset { get; set; }
        public int Index { get; set; }
        public bool IsLast { get; set; }
    }

    internal class FileChunkSource
    {
        public bool HasMore { get; private set; }

        private Stream stream; //the calling application instantiates IPlatformFile which controls the life of this stream
        private long fileLength;
        private long streamIndex;
        private int chunkCount;

        public FileChunkSource(IPlatformFile file)
        {
            stream = file.OpenRead();
            fileLength = file.Length;
            streamIndex = 0;
            chunkCount = 0;
            HasMore = true;
        }

        public FileChunk GetNextChunk(long requestedChunkSize)
        {
            try
            {
                int chunkSize = (int)Math.Min(requestedChunkSize, fileLength - streamIndex);
                byte[] content = new byte[chunkSize];
                stream.Read(content, 0, chunkSize);

                bool isLast = streamIndex + chunkSize >= fileLength;
                var chunk = new FileChunk { Content = content, Index = chunkCount, Offset = streamIndex, IsLast = isLast };

                streamIndex += chunkSize;
                chunkCount += 1;
                if (isLast)
                    HasMore = false;

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