using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Requests.Executors;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public abstract class AsyncUploaderBase : UploaderBase
    {
        protected AsyncUploaderBase(
            ShareFileClient client, 
            UploadSpecificationRequest uploadSpecificationRequest, 
            Stream stream,
            FileUploaderConfig config = null, 
            int? expirationDays = null)
            : base(client, uploadSpecificationRequest, stream, config, expirationDays)
        {
            HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
        }

        protected internal IAsyncRequestExecutor RequestExecutor
        {
            get
            {
                return Client.AsyncRequestExecutor ?? RequestExecutorFactory.GetAsyncRequestExecutor();
            }
        }

        protected async Task<UploadSpecification> CreateUpload(CancellationToken cancellationToken)
        {
            if (UploadSpecificationRequest.ProviderCapabilities == null)
            {
                var capabilities = Client.GetCachedCapabilities(UploadSpecificationRequest.Parent)
                        ?? (await CreateCapabilitiesQuery().ExecuteAsync().ConfigureAwait(false)).Feed;
                Client.SetCachedCapabilities(UploadSpecificationRequest.Parent, capabilities);

                UploadSpecificationRequest.ProviderCapabilities = capabilities;
            }

            var query = CreateUploadSpecificationQuery(UploadSpecificationRequest);

            return await query.ExecuteAsync(cancellationToken).ConfigureAwait(false);
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
            var buffer = new byte[Configuration.BufferSize];
            do
            {
                var bytesToRead = count < Configuration.BufferSize ? (int)count : Configuration.BufferSize;
                var bytesRead = await FileStream.ReadAsync(buffer, 0, bytesToRead);
                if (bytesRead > 0)
                {
                    localHash.Append(buffer, 0, bytesToRead);
                    HashProvider.Append(buffer, 0, bytesToRead);
                }
                count -= bytesRead;
            } while (count > 0);
            localHash.Finalize(new byte[1], 0, 0);
            FileStream.Seek(0, SeekOrigin.Begin);
            return localHash.GetComputedHashAsString();
        }

        public async Task<UploadResponse> UploadAsync(Dictionary<string, object> transferMetadata = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            await PrepareAsync(cancellationToken).ConfigureAwait(false);

            CancellationTokenSource uploadCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            UploadResponse response = null;
            try
            {
                progressReporter.StartReporting(transferMetadata, uploadCancellationSource.Token);
                response = await InternalUploadAsync(uploadCancellationSource.Token).ConfigureAwait(false);
            }
            finally
            {
                uploadCancellationSource.Cancel();
                uploadCancellationSource.Dispose();
            }
            progressReporter.ReportCompletion();

            try
            {
                FileStream.Dispose();
            }
            catch (Exception)
            {
                // Eat the exception, we tried to clean up.
            }

            return response;
        }

        protected abstract Task<UploadResponse> InternalUploadAsync(CancellationToken cancellationToken);

        public abstract Task PrepareAsync(CancellationToken cancellationToken = default(CancellationToken));
        
        protected async Task<UploadResponse> GetUploadResponseAsync(HttpResponseMessage responseMessage, string localHash = null)
        {
            var responseContent = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);
            return ValidateUploadResponse(responseMessage, responseContent, localHash);
        }
    }
}