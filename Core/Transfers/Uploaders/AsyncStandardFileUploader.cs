﻿using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Requests.Providers;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
#if Async
    public class AsyncStandardFileUploader : AsyncUploaderBase
    {
        public AsyncStandardFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null) 
            : base(client, uploadSpecificationRequest, file, config, expirationDays)
        {
            UploadSpecificationRequest.Raw = false;
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

                        using (var requestMessage = new HttpRequestMessage(
                                HttpMethod.Post,
                                GetChunkUriForStandardUploads()))
                        {
                            using (var multipartFormContent = new MultipartFormDataContent("upload-" + Guid.NewGuid().ToString("N")))
                            {
                                BaseRequestProvider.TryAddCookies(Client, requestMessage);

                                var streamContent = new StreamContentWithProgress(stream, OnProgress);
                                streamContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                                multipartFormContent.Add(streamContent, "File1", File.Name);
                                requestMessage.Content = multipartFormContent;

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
    }
#endif
}
