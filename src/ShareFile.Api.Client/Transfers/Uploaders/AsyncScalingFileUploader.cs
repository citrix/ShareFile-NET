using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.FileSystem;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
#if ASYNC
    public class AsyncScalingFileUploader : AsyncUploaderBase
    {
        private readonly ScalingPartUploader partUploader;

        private ActiveUploadState activeUploadState;

        public AsyncScalingFileUploader(
            ShareFileClient client,
            UploadSpecificationRequest uploadSpecificationRequest,
            IPlatformFile file,
            FileUploaderConfig config = null,
            int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {

            var chunkConfig = config != null ? config.PartConfig : new FilePartConfig();
            partUploader = new ScalingPartUploader(
                chunkConfig,
                Config.NumberOfThreads,
                ExecuteChunkUploadMessage,
                OnProgress,
                client.Logging);
        }

        public AsyncScalingFileUploader(
            ShareFileClient client,
            ActiveUploadState activeUploadState,
            UploadSpecificationRequest uploadSpecificationRequest,
            IPlatformFile file,
            FileUploaderConfig config = null)
            : this(client, uploadSpecificationRequest, file, config)
        {
            this.activeUploadState = activeUploadState;
            UploadSpecification = activeUploadState.UploadSpecification;
        }

        internal ScalingPartUploader PartUploader
        {
            get
            {
                return partUploader;
            }
        }

        public override long LastConsecutiveByteUploaded
        {
            get
            {
                return partUploader.LastConsecutiveByteUploaded;;
            }
        }

        private bool canRestart = true;

        protected override async Task<UploadResponse> InternalUploadAsync()
        {
            try
            {
                var offset = activeUploadState == null ? 0 : activeUploadState.BytesUploaded;
                await
                    partUploader.Upload(
                        File,
                        HashProvider,
                        UploadSpecification.ChunkUri.AbsoluteUri,
                        UploadSpecificationRequest.Raw,
                        offset,
                        CancellationToken).ConfigureAwait(false);
                return await FinishUploadAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // if the upload id was invalid, then the partial upload probably expired, so restart from the beginning
                var uploadException = ex as UploadException;
                var invalid = uploadException == null ? false : uploadException.IsInvalidUploadId;
                if (!canRestart || !invalid)
                {
                    throw;
                }
            }

            Progress.ResetBytesTransferred();
            activeUploadState = null;
            UploadSpecification = null;
            Prepared = false;
            canRestart = false;
            return await UploadAsync(TransferMetadata, CancellationToken).ConfigureAwait(false);
        }

        public override async Task PrepareAsync()
        {
            if (!Prepared)
            {
                if (UploadSpecification == null)
                {
                    UploadSpecification = await CreateUpload().ConfigureAwait(false);
                    if (UploadSpecification == null)
                    {
                        throw new UploadException("UploadSpecification cannot be null", UploadStatusCode.Unknown);
                    }
                }
                else
                {
                    // Run any query - this is to set auth headers that ShareFileClient handles by default, but the uploader doesn't.
                    // Let any exception fail the whole upload since if we can't reach parent, then we likely won't be able to upload.
                    var query = Client.Items.Get(UploadSpecificationRequest.Parent).Select("Id");
                    await query.ExecuteAsync(CancellationToken).ConfigureAwait(false);
                }
                partUploader.NumberOfThreads = Math.Min(
                    partUploader.NumberOfThreads,
                    UploadSpecification.MaxNumberOfThreads.GetValueOrDefault(1));
                partUploader.UploadSpecification = UploadSpecification;

                await CheckResumeAsync().ConfigureAwait(false);

                Prepared = true;
            }
        }

        private async Task<UploadResponse> FinishUploadAsync()
        {
            this.MarkProgressComplete();

            var client = GetHttpClient();
            var finishUri = this.GetFinishUriForThreadedUploads();
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, finishUri);

            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.AddDefaultHeaders(Client);

            var response = await RequestExecutor.SendAsync(
                client,
                requestMessage,
                HttpCompletionOption.ResponseContentRead,
                default(CancellationToken)).ConfigureAwait(false);

            return await GetUploadResponseAsync(response, HashProvider.GetComputedHashAsString()).ConfigureAwait(false);
        }

        private async Task ExecuteChunkUploadMessage(HttpRequestMessage requestMessage)
        {
            await TryPauseAsync(CancellationToken).ConfigureAwait(false);

            requestMessage.AddDefaultHeaders(Client);

            var client = GetHttpClient();
            using (
                var responseMessage =
                    await
                    RequestExecutor.SendAsync(
                        client,
                        requestMessage,
                        HttpCompletionOption.ResponseContentRead,
                        CancellationToken.GetValueOrDefault(System.Threading.CancellationToken.None)).ConfigureAwait(false))
            {
                if (Configuration.IsNetCore)
                {
                    OnProgress(requestMessage.Content.Headers.ContentLength.GetValueOrDefault());
                }
                string responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                ValidateChunkResponse(responseMessage, responseContent);
            }
        }
    }
#endif
}