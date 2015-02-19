using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Requests.Providers;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
#if Async
    public class AsyncScalingFileUploader : AsyncUploaderBase
    {
        private readonly ScalingPartUploader partUploader;

        public AsyncScalingFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            UploadSpecificationRequest.Raw = true;
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
                    UploadSpecification = await CreateUpload();
                }

                await CheckResumeAsync();

                Prepared = true;
            }
        }
        
        private async Task<UploadResponse> FinishUploadAsync()
        {
            var client = GetHttpClient();
            var finishUri = this.GetFinishUriForThreadedUploads();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, finishUri);

            requestMessage.Headers.Add("Accept", "application/json");
            BaseRequestProvider.TryAddCookies(Client, requestMessage);

            var response = await client.SendAsync(requestMessage);

            return await GetUploadResponseAsync(response);
        }

        private async Task ExecuteChunkUploadMessage(HttpRequestMessage requestMessage)
        {
            await TryPauseAsync(CancellationToken);

            BaseRequestProvider.TryAddCookies(Client, requestMessage);

            using(var responseMessage = await GetHttpClient().SendAsync(requestMessage, CancellationToken.GetValueOrDefault(System.Threading.CancellationToken.None)))
            {
                string response = await responseMessage.Content.ReadAsStringAsync();
                try
                {
                    var sfResponse = JsonConvert.DeserializeObject<ShareFileApiResponse<string>>(response);
                    if (sfResponse.Error)
                    {
                        throw new UploadException(sfResponse.ErrorMessage, sfResponse.ErrorCode);
                    }
                }
                catch (JsonSerializationException jEx)
                {
                    if (responseMessage.Content != null)
                    {
                        TryProcessFailedUploadResponse(response);
                    }

                    throw new UploadException("StorageCenter error: " + response, -1, jEx);
                }
            }
        }
    }
#endif
}
