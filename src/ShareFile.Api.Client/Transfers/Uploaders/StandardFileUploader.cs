using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;

using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#if NETFX_CORE
using ApplicationException = ShareFile.Api.Client.Exceptions.ApplicationException;
#endif

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class StandardFileUploader : SyncUploaderBase
    {
        public StandardFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
        }

        public override long LastConsecutiveByteUploaded
        {
            get
            {
                return 0;
            }
        }

        public override UploadResponse Upload(Dictionary<string, object> transferMetadata = null, CancellationToken? cancellationToken = null)
        {
            SetUploadSpecification();

            int tryCount = 0;
            Stream stream = File.OpenRead();
            while (true)
            {
                try
                {
                    TryPause();
                    cancellationToken.ThrowIfRequested();
                    var httpClient = GetHttpClient();

                    using (var requestMessage = new HttpRequestMessage(
                            HttpMethod.Post,
                            GetChunkUriForStandardUploads()))
                    {
                        using (var streamContent = new StreamContentWithProgress(new NoDisposeStream(stream), OnProgress, cancellationToken.GetValueOrDefault()))
                        {
#if ASYNC
                            streamContent.TryPauseAction = TryPauseAsync;
#else
                            streamContent.TryPauseAction = TryPause;
#endif
                            requestMessage.AddDefaultHeaders(Client);
                            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                            requestMessage.Content = streamContent;

                            if (!UploadSpecificationRequest.Raw)
                            {
                                var multiPartContent = new MultipartFormDataContent();
                                multiPartContent.Add(streamContent, "Filedata", File.Name);
                                requestMessage.Content = multiPartContent;
                            }
                            
                            var responseMessage =
                                httpClient.SendAsync(requestMessage, CancellationToken.None).WaitForTask();

                            MarkProgressComplete();

                            return GetUploadResponse(responseMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (tryCount >= 3 || !stream.CanSeek || ex is TaskCanceledException)
                    {
                        throw;
                    }
                    else
                    {
                        tryCount += 1;
                        stream.Seek(0, SeekOrigin.Begin);
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
