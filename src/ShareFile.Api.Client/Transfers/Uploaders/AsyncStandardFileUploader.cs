using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.FileSystem;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
#if ASYNC
    public class AsyncStandardFileUploader : AsyncUploaderBase
    {
        public AsyncStandardFileUploader(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null) 
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

        public override async Task PrepareAsync()
        {
            if (!Prepared)
            {
                if (UploadSpecification == null)
                {
                    UploadSpecification = await CreateUpload().ConfigureAwait(false);
                }

                await CheckResumeAsync().ConfigureAwait(false);

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
                    await TryPauseAsync(CancellationToken).ConfigureAwait(false);
                    if (CancellationToken.GetValueOrDefault().IsCancellationRequested)
                    {
                        throw new TaskCanceledException();
                    }
                    var httpClient = GetHttpClient();

                    using (var requestMessage = new HttpRequestMessage(
                            HttpMethod.Post,
                            GetChunkUriForStandardUploads()))
                    {
                        using (var streamContent = new StreamContentWithProgress(new NoDisposeStream(stream), OnProgress, CancellationToken.GetValueOrDefault()))
                        {
                            streamContent.TryPauseAction = TryPauseAsync;
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
                                await
                                httpClient.SendAsync(
                                    requestMessage,
                                    CancellationToken.GetValueOrDefault(System.Threading.CancellationToken.None)).ConfigureAwait(false);

                            MarkProgressComplete();

                            return await GetUploadResponseAsync(responseMessage).ConfigureAwait(false);
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
    }
#endif
                        }
