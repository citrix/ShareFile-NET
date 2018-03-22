using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using ShareFile.Api.Client.Models;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.Security.Cryptography;
using System.Threading;

using ShareFile.Api.Client.Requests.Executors;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public abstract class SyncUploaderBase : UploaderBase
    {
        public abstract void Prepare();

        protected SyncUploaderBase(
            ShareFileClient client,
            UploadSpecificationRequest uploadSpecificationRequest, 
            Stream stream,
            FileUploaderConfig config = null,
            int? expirationDays = null)
            : base(client, uploadSpecificationRequest, stream, config, expirationDays)
        {
            HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
        }

        protected internal ISyncRequestExecutor RequestExecutor
        {
            get
            {
                return Client.SyncRequestExecutor ?? RequestExecutorFactory.GetSyncRequestExecutor();
            }
        }

        protected abstract UploadResponse InternalUpload(CancellationToken cancellationToken);

        public UploadResponse Upload(Dictionary<string, object> transferMetadata = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            CancellationTokenSource uploadCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            UploadResponse response;
            try
            {
                progressReporter.StartReporting(transferMetadata, uploadCancellationSource.Token);
                response = InternalUpload(uploadCancellationSource.Token);
            }
            finally
            {
                uploadCancellationSource.Cancel();
                uploadCancellationSource.Dispose();
            }
            progressReporter.ReportCompletion();
            return response;
        }

        protected void CheckResume()
        {
            if (UploadSpecification.IsResume)
            {
                if (UploadSpecification.ResumeFileHash != CalculateHash(UploadSpecification.ResumeOffset))
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

        protected string CalculateHash(long count)
        {
            do
            {
                var buffer = new byte[Configuration.BufferSize];

                if (count < buffer.Length)
                {
                    buffer = new byte[count];
                }

                var bytesRead = FileStream.Read(buffer, 0, buffer.Length);

                if (bytesRead > 0)
                {
                    HashProvider.Append(buffer, 0, buffer.Length);
                }

                count -= bytesRead;

            } while (count > 0);

            FileStream.Seek(0, SeekOrigin.Begin);
            return HashProvider.GetComputedHashAsString();
        }

        protected UploadSpecification SetUploadSpecification()
        {
            if (UploadSpecification == null)
            {
                if (UploadSpecificationRequest.ProviderCapabilities == null)
                {
                    var capabilities = Client.GetCachedCapabilities(UploadSpecificationRequest.Parent)
                        ?? CreateCapabilitiesQuery().Execute().Feed;
                    Client.SetCachedCapabilities(UploadSpecificationRequest.Parent, capabilities);

                    UploadSpecificationRequest.ProviderCapabilities = capabilities;
                }

                UploadSpecification = CreateUploadSpecificationQuery(UploadSpecificationRequest).Execute();
            }
            return UploadSpecification;
        }

        protected UploadResponse GetUploadResponse(HttpResponseMessage responseMessage, string localHash = null)
        {
            string responseContent = responseMessage.Content.ReadAsStringAsync().WaitForTask();
            return ValidateUploadResponse(responseMessage, responseContent, localHash);
        }
    }
}