using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Requests.Providers;
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
        private ScalingPartUploader partUploader;

        public ScalingFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            UploadSpecificationRequest.Raw = true;
            var partConfig = config != null ? config.PartConfig : new FilePartConfig();
            partUploader = new ScalingPartUploader(partConfig, Config.NumberOfThreads,
                requestMessage => Task.Factory.StartNew(() => ExecuteChunkUploadMessage(requestMessage)),
                OnProgress);
        }

        public override UploadResponse Upload(Dictionary<string, object> transferMetadata = null)
        {
            try
            {
                SetUploadSpecification();
                var uploads = partUploader.Upload(File, HashProvider, UploadSpecification.ChunkUri.AbsoluteUri);
                uploads.Wait();
                return FinishUpload();
            }
            catch(AggregateException aggEx)
            {
                throw aggEx.Unwrap();
            }
        }
        
        private void ExecuteChunkUploadMessage(HttpRequestMessage requestMessage)
        {
            TryPause();

            BaseRequestProvider.TryAddCookies(Client, requestMessage);

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

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, finishUri);
            requestMessage.Headers.Add("Accept", "application/json");
            BaseRequestProvider.TryAddCookies(Client, requestMessage);

            var response = client.SendAsync(requestMessage).WaitForTask();

            return GetUploadResponse(response);
        }
        
        public override void Prepare()
        {
            throw new NotImplementedException();
        }

    }
}
