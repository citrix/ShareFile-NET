using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Cryptography;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
#if Async
    public class AsyncScalingFileUploader : AsyncUploaderBase
    {
        private ScalingPartUploader partUploader;

        public AsyncScalingFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            var chunkConfig = config != null ? config.PartConfig : new FilePartConfig();
            partUploader = new ScalingPartUploader(chunkConfig, Config.NumberOfThreads,
                ExecuteChunkUploadMessage,
                (bytesTransferred, finished) => OnProgress(bytesTransferred));        
        }

        protected override async Task<UploadResponse> InternalUploadAsync()
        {
            await partUploader.Upload(File, HashProvider, UploadSpecification.ChunkUri.AbsoluteUri);
            return await FinishUploadAsync();
        }

        public override async Task PrepareAsync()
        {
            if (!Prepared)
            {
                if (UploadSpecification == null)
                {
                    UploadSpecification = await CreateUpload(UploadSpecificationRequest);
                }

                await CheckResumeAsync();

                Prepared = true;
            }
        }
        
        private async Task<UploadResponse> FinishUploadAsync()
        {
            var client = GetHttpClient();
            var finishUri = this.GetFinishUriForThreadedUploads();
            var message = new HttpRequestMessage(HttpMethod.Get, finishUri);

            message.Headers.Add("Accept", "application/json");

            var response = await client.SendAsync(message);

            return await GetUploadResponseAsync(response);
        }

        private async Task ExecuteChunkUploadMessage(HttpRequestMessage requestMessage)
        {
            using(var responseMessage = await GetHttpClient().SendAsync(requestMessage))
            {
                string response = await responseMessage.Content.ReadAsStringAsync();
                try
                {
                    var sfResponse = JsonConvert.DeserializeObject<ShareFileApiResponse<string>>(response);
                    if (sfResponse.Error)
                        throw new UploadException(sfResponse.ErrorMessage, sfResponse.ErrorCode);
                }
                catch(JsonSerializationException jEx)
                {
                    throw new UploadException("StorageCenter error: " + response, -1, jEx);
                }
            }
        }
    }
#endif
}
