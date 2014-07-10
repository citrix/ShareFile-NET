#if !Async
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;
using System;
using System.Collections.Generic;

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
            int tryCount = 0;
            Exception lastException = null;

            while (tryCount < 3)
            {
                try
                {
                    var httpClient = GetHttpClient();
                    var boundaryGuid = "upload-" + Guid.NewGuid().ToString("N");
                    var requestMessage = new HttpRequestMessage(HttpMethod.Post, GetChunkUriForStandardUploads());
                    var multipartFormContent = new MultipartFormDataContent(boundaryGuid);

                    var streamContent = new StreamContent(File.OpenRead(), MaxBufferLength);
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipartFormContent.Add(streamContent, "File1", File.Name);

                    requestMessage.Content = multipartFormContent;

                    var responseMessage = httpClient.SendAsync(requestMessage, CancellationToken.None).WaitForTask();

                    return GetUploadResponse(responseMessage);
                }
                catch (Exception exception)
                {
                    lastException = exception;
                    tryCount++;
                }
            }

            throw new ApplicationException("Upload failed after 3 tries", lastException);
        }

        public override void Prepare()
        {
            if (!Prepared)
            {
                if (UploadSpecification == null)
                {
                    UploadSpecification = CreateUploadSpecificationQuery(UploadSpecificationRequest).Execute();
                }

                CheckResume();

                Prepared = true;
            }
        }
    }
}
#endif
