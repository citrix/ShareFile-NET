using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Extensions.Tasks;
using System;
using System.IO;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Exceptions;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class ScalingFileUploader : SyncUploaderBase
    {
        private readonly ScalingPartUploader partUploader;

        private ActiveUploadState activeUploadState;

        public ScalingFileUploader(
            ShareFileClient client,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null,
            int? expirationDays = null)
            : base(client, uploadSpecificationRequest, stream, config, expirationDays)
        {
            var partConfig = config != null ? config.PartConfig : new FilePartConfig();
            partUploader = new ScalingPartUploader(
                partConfig,
                Config.NumberOfThreads,
                (requestMessage, cancelToken) => Task.Factory.StartNew(() => ExecuteChunkUploadMessage(requestMessage, cancelToken)),
                progressReporter.ReportProgress,
                client.Logging);
        }
        
        public ScalingFileUploader(
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
                return partUploader.LastConsecutiveByteUploaded;
            }
        }

        private bool canRestart = true;

        protected override UploadResponse InternalUpload(CancellationToken cancellationToken)
        {
            try
            {
                var offset = activeUploadState == null ? 0 : activeUploadState.BytesUploaded;
                SetUploadSpecification();
                partUploader.NumberOfThreads = Math.Min(
                    partUploader.NumberOfThreads,
                    UploadSpecification.MaxNumberOfThreads.GetValueOrDefault(1));
                partUploader.UploadSpecification = UploadSpecification;
                var uploads = partUploader.Upload(
                    FileStream,
                    UploadSpecificationRequest.FileName,
                    HashProvider,
                    UploadSpecification.ChunkUri.AbsoluteUri,
                    UploadSpecificationRequest.Raw,
                    offset,
                    cancellationToken);
                uploads.Wait();
                return FinishUpload();
            }
            catch (Exception ex)
            {
                var agg = ex as AggregateException;
                if (agg != null)
                {
                    ex = agg.Unwrap();
                }
                //if (canRestart && ((ex as UploadException)?.IsInvalidUploadId).GetValueOrDefault())
                if (canRestart && ((ex is UploadException) && ((UploadException)ex).IsInvalidUploadId))
                {
                    activeUploadState = null;
                    UploadSpecification = null;
                    Prepared = false;
                    canRestart = false;
                    return Upload();
                }
                throw;
            }
        }

        private void ExecuteChunkUploadMessage(HttpRequestMessage requestMessage, CancellationToken cancellationToken)
        {
            TryPause();

            requestMessage.AddDefaultHeaders(Client);

            var client = GetHttpClient();
            using (var responseMessage = RequestExecutor.Send(client, requestMessage, HttpCompletionOption.ResponseContentRead))
            {
                if (Configuration.IsNetCore)
                {
                    progressReporter.ReportProgress(requestMessage.Content.Headers.ContentLength.GetValueOrDefault());
                }
                string responseContent = responseMessage.Content.ReadAsStringAsync().WaitForTask();
                ValidateChunkResponse(responseMessage, responseContent);
            }
        }

        private UploadResponse FinishUpload()
        {
            var finishUri = GetFinishUriForThreadedUploads();
            var client = GetHttpClient();

            var requestMessage = new HttpRequestMessage(HttpMethod.Get, finishUri);
            requestMessage.Headers.Add("Accept", "application/json");
            requestMessage.AddDefaultHeaders(Client);

            var response = RequestExecutor.Send(client, requestMessage, HttpCompletionOption.ResponseContentRead);

            return GetUploadResponse(response, HashProvider.GetComputedHashAsString());
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }
    }
}