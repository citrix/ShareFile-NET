using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Extensions.Tasks;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShareFile.Api.Client.Models;

#if NETFX_CORE
using ApplicationException = ShareFile.Api.Client.Exceptions.ApplicationException;
#endif

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class StandardFileUploader : SyncUploaderBase
    {
        public StandardFileUploader(
            ShareFileClient client,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null,
            int? expirationDays = null)
            : base(client, uploadSpecificationRequest, stream, config, expirationDays)
        { }

        public StandardFileUploader(
            ShareFileClient client,
            UploadSpecification uploadSpecification,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null)
            : this(client, uploadSpecificationRequest, stream, config, expirationDays: null)
        {
            UploadSpecification = uploadSpecification;
        }
        
        public override long LastConsecutiveByteUploaded
        {
            get
            {
                return 0;
            }
        }

        protected override UploadResponse InternalUpload(CancellationToken cancellationToken)
        {
            SetUploadSpecification();

            int tryCount = 0;
            while (true)
            {
                try
                {
                    TryPause();
                    if (cancellationToken.IsCancellationRequested)
                        throw new TaskCanceledException();

                    var httpClient = GetHttpClient();

                    using (var requestMessage = new HttpRequestMessage(
                            HttpMethod.Post,
                            GetChunkUriForStandardUploads()))
                    {
                        using (var streamContent = new StreamContentWithProgress(new NoDisposeStream(FileStream), progressReporter.ReportProgress, cancellationToken))
                        {
                            streamContent.TryPauseAction = TryPauseAsync;
                            requestMessage.AddDefaultHeaders(Client);
                            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                            requestMessage.Content = streamContent;

                            if (!UploadSpecificationRequest.Raw)
                            {
                                var multiPartContent = new MultipartFormDataContent();
                                multiPartContent.Add(streamContent, "Filedata", UploadSpecificationRequest.FileName);
                                requestMessage.Content = multiPartContent;
                            }
                            
                            var responseMessage =
                                httpClient.SendAsync(requestMessage, CancellationToken.None).WaitForTask();
                            
                            return GetUploadResponse(responseMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (tryCount >= 3 || !FileStream.CanSeek || ex is TaskCanceledException)
                    {
                        throw;
                    }
                    else
                    {
                        tryCount += 1;
                        FileStream.Seek(0, SeekOrigin.Begin);
                    }
                }
            }
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }
    }
}
