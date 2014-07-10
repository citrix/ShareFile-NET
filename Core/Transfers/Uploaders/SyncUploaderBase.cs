using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Security.Cryptography;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public abstract class SyncUploaderBase : UploaderBase
    {
        public abstract UploadResponse Upload(Dictionary<string, object> transferMetadata = null);
        public abstract void Prepare();

        protected SyncUploaderBase(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
            : base(client, uploadSpecificationRequest, file, expirationDays)
        {
            Config = config ?? new FileUploaderConfig();
            HashProvider = MD5HashProviderFactory.GetHashProvider().CreateHash();
            Progress = new TransferProgress
            {
                TransferId = Guid.NewGuid().ToString(),
                BytesTransferred = 0,
                BytesRemaining = uploadSpecificationRequest.FileSize,
                TotalBytes = uploadSpecificationRequest.FileSize
            };
        }

        public FileUploaderConfig Config { get; private set; }
        public TransferProgress Progress { get; set; }

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

        protected UploadResponse GetUploadResponse(HttpResponseMessage responseMessage)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                using (var responseStream = responseMessage.Content.ReadAsStreamAsync().WaitForTask())
                using (var textReader = new JsonTextReader(new StreamReader(responseStream)))
                {
                    var uploadResponse = new JsonSerializer().Deserialize<ShareFileApiResponse<UploadResponse>>(textReader);

                    if (uploadResponse.Error)
                    {
                        throw new UploadException(uploadResponse.ErrorMessage, uploadResponse.ErrorCode);
                    }

                    return uploadResponse.Value;
                }
            }

            throw new UploadException("Error completing upload.", -1);
        }

        protected internal override HttpClient GetHttpClient()
        {
            return new HttpClient(GetHttpClientHandler())
            {
                Timeout = new TimeSpan(0, 0, 0, 0, Config.HttpTimeout)
            };
        }
    }
}
