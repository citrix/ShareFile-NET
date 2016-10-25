using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;

using ShareFile.Api.Client.Enums;
using ShareFile.Api.Models;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Cryptography;
using System.Threading;

using ShareFile.Api.Client.Requests.Executors;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public abstract class SyncUploaderBase : UploaderBase
    {
        public abstract UploadResponse Upload(Dictionary<string, object> transferMetadata = null, CancellationToken? cancellationToken = null);
        public abstract void Prepare();

        protected SyncUploaderBase(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, expirationDays)
        {
            Config = config ?? new FileUploaderConfig();
            HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
        }

        public FileUploaderConfig Config { get; private set; }

        protected internal ISyncRequestExecutor RequestExecutor
        {
            get
            {
                return Client.SyncRequestExecutor ?? RequestExecutorFactory.GetSyncRequestExecutor();
            }
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
            using (var fileStream = File.OpenRead())
            {
                do
                {
                    var buffer = new byte[65536];

                    if (count < buffer.Length)
                    {
                        buffer = new byte[count];
                    }

                    var bytesRead = fileStream.Read(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        HashProvider.Append(buffer, 0, buffer.Length);
                    }

                    count -= bytesRead;

                } while (count > 0);
            }

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
    }
}