using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
#if Async
    public abstract class AsyncUploaderBase : UploaderBase
    {
        protected AsyncUploaderBase(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, expirationDays)
        {
            Config = config ?? new FileUploaderConfig();

            HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
            Progress = new TransferProgress
            {
                TransferId = Guid.NewGuid().ToString(),
                BytesTransferred = 0
            };

            Progress.BytesRemaining = Progress.TotalBytes = uploadSpecificationRequest.FileSize;
        }

        public FileUploaderConfig Config { get; protected set; }

        public TransferProgress Progress { get; set; }
        protected CancellationToken? CancellationToken { get; set; }

        protected async Task<UploadSpecification> CreateUpload()
        {
            if (UploadSpecificationRequest.ProviderCapabilities == null)
            {
                UploadSpecificationRequest.ProviderCapabilities =
                    (await
                        Client.Capabilities.Get()
                            .WithBaseUri(UploadSpecificationRequest.Parent)
                            .ExecuteAsync(CancellationToken)).Feed;
            }

            var query = CreateUploadSpecificationQuery(UploadSpecificationRequest);

            return await query.ExecuteAsync(CancellationToken);
        }

        protected async Task CheckResumeAsync()
        {
            if (UploadSpecification.IsResume)
            {
                if (UploadSpecification.ResumeFileHash != await CalculateHashAsync(UploadSpecification.ResumeOffset))
                {
                    HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
                    UploadSpecification.ResumeIndex = 0;
                    UploadSpecification.ResumeOffset = 0;
                }
                else
                {
                    UploadSpecification.ResumeIndex += 1;
                }
            }
        }

        protected async Task<string> CalculateHashAsync(long count)
        {
            var localHash = MD5HashProviderFactory.GetHashProvider().CreateHash();
            using (var fileStream = await File.OpenReadAsync())
            {
                var buffer = new byte[MaxBufferLength];
                do
                {
                    var bytesToRead = count < MaxBufferLength ? (int)count : MaxBufferLength;
                    var bytesRead = fileStream.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        localHash.Append(buffer, 0, bytesToRead);
                        HashProvider.Append(buffer, 0, bytesToRead);
                    }
                    count -= bytesRead;
                } while (count > 0);
            }
            localHash.Finalize(new byte[1], 0, 0);
            return localHash.GetComputedHashAsString();
        }

        public async Task<UploadResponse> UploadAsync(Dictionary<string, object> transferMetadata = null, CancellationToken? cancellationToken = null)
        {
            await PrepareAsync();

            TransferMetadata = transferMetadata ?? new Dictionary<string, object>();
            Progress.TransferMetadata = TransferMetadata;
            CancellationToken = cancellationToken;

            var response = await InternalUploadAsync();

            try
            {
                var stream = await File.OpenReadAsync();
                stream.Dispose();
            }
            catch (Exception)
            {
                // Eat the exception, we tried to clean up.
            }

            return response;
        }

        protected abstract Task<UploadResponse> InternalUploadAsync();
        public abstract Task PrepareAsync();

        protected void OnProgress(int bytesTransferred)
        {
            Progress.BytesTransferred += bytesTransferred;
            NotifyProgress(Progress);
        }

        protected bool IsCancellationRequested()
        {
            if (CancellationToken == null) return false;

            return CancellationToken.Value.IsCancellationRequested;
        }

        private HttpClient httpClient;

        protected internal override HttpClient GetHttpClient()
        {
            if (httpClient == null)
            {
                httpClient = new HttpClient(GetHttpClientHandler())
                {
                    Timeout = new TimeSpan(0, 0, 0, 0, Config.HttpTimeout)
                };
            }
            return httpClient;
        }

        protected async Task<UploadResponse> GetUploadResponseAsync(HttpResponseMessage responseMessage)
        {
            var response = await responseMessage.Content.ReadAsStringAsync();
            using (var textReader = new JsonTextReader(new StringReader(response)))
            {
                try
                {
                    var uploadResponse = new JsonSerializer().Deserialize<ShareFileApiResponse<UploadResponse>>(textReader);

                    if (uploadResponse.Error)
                    {
                        throw new UploadException(uploadResponse.ErrorMessage, uploadResponse.ErrorCode);
                    }

                    return uploadResponse.Value;

                }
                catch (JsonSerializationException jEx)
                {
                    TryProcessFailedUploadResponse(response);

                    throw new UploadException("StorageCenter error: " + response, -1, jEx);
                }
            }
        }
    }
#endif
}
