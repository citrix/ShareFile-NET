using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Requests.Providers;
using System;
using System.Collections.Generic;
#if Portable || NETFX_CORE
using ApplicationException = ShareFile.Api.Client.Exceptions.ApplicationException;
#endif

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public class StandardFileUploader : SyncUploaderBase
    {
        public StandardFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null) 
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            UploadSpecificationRequest.Raw = false;
        }

        public override UploadResponse Upload(Dictionary<string, object> transferMetadata = null)
        {
            SetUploadSpecification();

            int tryCount = 0;
            Exception lastException = null;

            Stream stream = null;
            try
            {
                stream = File.OpenRead();
                while (tryCount < 3)
                {
                    try
                    {
                        var httpClient = GetHttpClient();
                        var boundaryGuid = "upload-" + Guid.NewGuid().ToString("N");
                        var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetChunkUriForStandardUploads());

                        BaseRequestProvider.TryAddCookies(Client, requestMessage);

                        var multipartFormContent = new MultipartFormDataContent(boundaryGuid);

                        var streamContent = new StreamContentWithProgress(
                            stream,
                            OnProgress);

                        streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                        multipartFormContent.Add(streamContent, "File1", File.Name);

                        requestMessage.Content = multipartFormContent;

                        var responseMessage = httpClient.SendAsync(requestMessage, CancellationToken.None).WaitForTask();

                        MarkProgressComplete();
                        
                        return GetUploadResponse(responseMessage);
                    }
                    catch (Exception exception)
                    {
                        lastException = exception;
                        stream.Seek(0, SeekOrigin.Begin);
                        tryCount++;
                    }
                }
            }
            finally
            {
                if (stream != null)
                {
                    stream.Dispose();
                }
            }

            throw new ApplicationException("Upload failed after 3 tries", lastException);
        }

        public override void Prepare()
        {
            throw new NotImplementedException();
        }
    }
}
