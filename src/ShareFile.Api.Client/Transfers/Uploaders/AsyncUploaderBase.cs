using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Requests.Executors;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
#if ASYNC
    public abstract class AsyncUploaderBase : UploaderBase
    {
        protected AsyncUploaderBase(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, expirationDays)
        {
            Config = config ?? new FileUploaderConfig();

            HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
        }

        public FileUploaderConfig Config { get; protected set; }

        protected CancellationToken? CancellationToken { get; set; }

        protected internal IAsyncRequestExecutor RequestExecutor
        {
            get
            {
                return Client.AsyncRequestExecutor ?? RequestExecutorFactory.GetAsyncRequestExecutor();
            }
        }

        protected async Task<UploadSpecification> CreateUpload()
        {
            if (UploadSpecificationRequest.ProviderCapabilities == null)
            {
                var capabilities = Client.GetCachedCapabilities(UploadSpecificationRequest.Parent)
                        ?? (await CreateCapabilitiesQuery().ExecuteAsync().ConfigureAwait(false)).Feed;
                Client.SetCachedCapabilities(UploadSpecificationRequest.Parent, capabilities);

                UploadSpecificationRequest.ProviderCapabilities = capabilities;
            }

            var query = CreateUploadSpecificationQuery(UploadSpecificationRequest);

            return await query.ExecuteAsync(CancellationToken).ConfigureAwait(false);
        }

        protected async Task CheckResumeAsync()
        {
            if (UploadSpecification.IsResume)
            {
                if (UploadSpecification.ResumeFileHash != await CalculateHashAsync(UploadSpecification.ResumeOffset).ConfigureAwait(false))
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
            var fileStream = await File.OpenReadAsync().ConfigureAwait(false);
            {
                var buffer = new byte[DefaultBufferLength];
                do
                {
                    var bytesToRead = count < DefaultBufferLength ? (int)count : DefaultBufferLength;
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
            fileStream.Seek(0, SeekOrigin.Begin);
            return localHash.GetComputedHashAsString();
        }

        public async Task<UploadResponse> UploadAsync(Dictionary<string, object> transferMetadata = null, CancellationToken? cancellationToken = null)
        {
            await PrepareAsync().ConfigureAwait(false);

            TransferMetadata = transferMetadata ?? new Dictionary<string, object>();
            Progress.TransferMetadata = TransferMetadata;
            CancellationToken = cancellationToken;

            var response = await InternalUploadAsync().ConfigureAwait(false);

            try
            {
                var stream = await File.OpenReadAsync().ConfigureAwait(false);
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

        protected async Task<UploadResponse> GetUploadResponseAsync(HttpResponseMessage responseMessage, string localHash = null)
        {
            var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ValidateUploadResponse(responseMessage, responseContent, localHash);
        }
    }
#endif
}