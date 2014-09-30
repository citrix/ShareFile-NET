#if !Async
using System;
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

        public ScalingFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            targetChunkUploadTime = TimeSpan.FromSeconds(30);
        }

        public override UploadResponse Upload(Dictionary<string, object> transferMetadata = null)
        {
            SetUploadSpecification();

            var chunkSource = new FileChunkSource(File);   
        }

        private void UploadChunk(FileChunk chunk)
        {

        }

        private int CalculateChunkIncrement(long chunkSize, TimeSpan targetTime, TimeSpan elapsedTime)
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
                    int chunkIncrement = CalculateChunkIncrement(workerChunk.Content.Length, targetChunkUploadTime, elapsed);
                    currentChunkSize += chunkIncrement; //this isn't thread-safe, but nothing horrible should happen if it gets clobbered
                    return ChunkUploadResult.Success;
                };

            var workers = new SemaphoreSlim(Config.NumberOfThreads);
            bool giveUp = false;
            while (!giveUp && chunkSource.HasMore)
            {
                workers.Wait(); //deadlock timeout here?
                var chunk = chunkSource.GetNextChunk(currentChunkSize);
                if (chunk == null || giveUp)
                    break; //stream is busted, give up

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

        public FileChunkSource(IPlatformFile file)
        {

        }

        public FileChunk GetNextChunk(long chunkSize)
        {

        }
    }
}
#endif