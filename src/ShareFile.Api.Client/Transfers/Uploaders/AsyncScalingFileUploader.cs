using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class AsyncScalingFileUploader : AsyncUploaderBase
    {
        private readonly ScalingPartUploader partUploader;

        private ActiveUploadState activeUploadState;

        public AsyncScalingFileUploader(
            ShareFileClient client,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null,
            int? expirationDays = null)
            : base(client, uploadSpecificationRequest, stream, config, expirationDays)
        {
            var chunkConfig = config != null ? config.PartConfig : new FilePartConfig();
            partUploader = new ScalingPartUploader(
                chunkConfig,
                Config.NumberOfThreads,
                ExecuteChunkUploadMessage,
                progressReporter.ReportProgress,
                client.Logging);
        }

        public AsyncScalingFileUploader(
            ShareFileClient client,
            ActiveUploadState activeUploadState,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null)
            : this(client, uploadSpecificationRequest, stream, config)
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

        protected override async Task<UploadResponse> InternalUploadAsync(CancellationToken cancellationToken)
        {
            try
            {
                var offset = activeUploadState == null ? 0 : activeUploadState.BytesUploaded;
                await
                    partUploader.Upload(
                        FileStream,
                        UploadSpecificationRequest.FileName,
                        HashProvider,
                        UploadSpecification.ChunkUri.AbsoluteUri,
                        UploadSpecificationRequest.Raw,
                        offset,
                        cancellationToken).ConfigureAwait(false);
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

            progressReporter.ResetProgress();
            activeUploadState = null;
            UploadSpecification = null;
            Prepared = false;
            canRestart = false;
            return await UploadAsync(null, cancellationToken).ConfigureAwait(false);
        }

        public override async Task PrepareAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!Prepared)
            {
                if (UploadSpecification == null)
                {
                    UploadSpecification = await CreateUpload(cancellationToken).ConfigureAwait(false);
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
                    await query.ExecuteAsync(cancellationToken).ConfigureAwait(false);
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

        private async Task ExecuteChunkUploadMessage(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            await TryPauseAsync(cancellationToken).ConfigureAwait(false);

            requestMessage.AddDefaultHeaders(Client);

            var client = GetHttpClient();
            using (
                var responseMessage =
                    await
                    RequestExecutor.SendAsync(
                        client,
                        requestMessage,
                        HttpCompletionOption.ResponseContentRead,
                        cancellationToken).ConfigureAwait(false))
            {
                if (Configuration.IsNetCore)
                {
                    progressReporter.ReportProgress(requestMessage.Content.Headers.ContentLength.GetValueOrDefault());
                }
                string responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
                ValidateChunkResponse(responseMessage, responseContent);
            }
        }
    }
}