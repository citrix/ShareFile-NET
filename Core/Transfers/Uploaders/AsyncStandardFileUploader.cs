using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.FileSystem;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
#if Async
    public class AsyncStandardFileUploader : AsyncUploaderBase
    {
        public AsyncStandardFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null) 
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            UploadSpecificationRequest.Raw = true;
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

        protected override async Task<UploadResponse> InternalUploadAsync()
        {
            int tryCount = 0;
            Stream stream = File.OpenRead();
            while (true)
            {
                try
                {
                    var httpClient = GetHttpClient();

                    using (var requestMessage = new HttpRequestMessage(
                            HttpMethod.Post,
                            GetChunkUriForStandardUploads()))
                    {
                        using (var streamContent = new StreamContentWithProgress(new NoDisposeStream(stream), OnProgress))
                        {
                            requestMessage.AddDefaultHeaders(Client);
                            streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                            requestMessage.Content = streamContent;

                            var responseMessage =
                                await
                                httpClient.SendAsync(
                                    requestMessage,
                                    CancellationToken.GetValueOrDefault(System.Threading.CancellationToken.None));

                            MarkProgressComplete();

                            return await GetUploadResponseAsync(responseMessage);
                        }
                    }
                }
                catch (Exception)
                {
                    if (tryCount >= 3)
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
    }
#endif
}
