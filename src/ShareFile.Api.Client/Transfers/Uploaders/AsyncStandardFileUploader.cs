using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ShareFile.Api.Client.Extensions;
using System.Threading;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class AsyncStandardFileUploader : AsyncUploaderBase
    {
        public AsyncStandardFileUploader(
            ShareFileClient client,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null,
            int? expirationDays = null)
            : base(client, uploadSpecificationRequest, stream, config, expirationDays)
        { }
        
        public override long LastConsecutiveByteUploaded
        {
            get
            {
                return 0;
            }
        }

        public override async Task PrepareAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!Prepared)
            {
                if (UploadSpecification == null)
                {
                    UploadSpecification = await CreateUpload(cancellationToken).ConfigureAwait(false);
                }

                await CheckResumeAsync().ConfigureAwait(false);

                Prepared = true;
            }
        }

        protected override async Task<UploadResponse> InternalUploadAsync(CancellationToken cancellationToken)
        {
            int tryCount = 0;
            while (true)
            {
                try
                {
                    await TryPauseAsync(cancellationToken).ConfigureAwait(false);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
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

                            var responseMessage = await httpClient.SendAsync(requestMessage, cancellationToken).ConfigureAwait(false);                            
                            return await GetUploadResponseAsync(responseMessage).ConfigureAwait(false);
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
    }
}
