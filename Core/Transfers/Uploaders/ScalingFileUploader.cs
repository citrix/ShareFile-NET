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
        private ScalingChunkUploader chunkUploader;

        public ScalingFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            var chunkConfig = config != null ? config.ChunkConfig : new FileChunkConfig();
            chunkUploader = new ScalingChunkUploader(chunkConfig, Config.NumberOfThreads,
                requestMessage => Task.Factory.StartNew(() => ExecuteChunkUploadMessage(requestMessage)),
                UpdateProgress);
        }

        public override UploadResponse Upload(Dictionary<string, object> transferMetadata = null)
        {
            SetUploadSpecification();
            var uploads = chunkUploader.Upload(File, HashProvider, UploadSpecification.ChunkUri.AbsoluteUri);
            uploads.Wait();
            return FinishUpload();
        }
        
        private void ExecuteChunkUploadMessage(HttpRequestMessage requestMessage)
        {
            using (var responseMessage = GetHttpClient().SendAsync(requestMessage).WaitForTask())
            {
                DeserializeShareFileApiResponse<string>(responseMessage);
                //no exception = success?
            }
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

        private void UpdateProgress(int bytesUploaded, bool finished)
        {
            Progress.BytesTransferred += bytesUploaded;
            Progress.BytesRemaining -= bytesUploaded;
            Progress.Complete = finished;
            NotifyProgress(Progress);
        }
        
        public override void Prepare()
        {
            throw new NotImplementedException();
        }

    }
}
