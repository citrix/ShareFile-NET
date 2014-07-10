using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using Newtonsoft.Json;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public abstract class UploaderBase : TransfererBase
    {
        protected UploaderBase(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, int? expirationDays)
        {
            Client = client;
            UploadSpecificationRequest = uploadSpecificationRequest;
            File = file;

            ExpirationDays = expirationDays;
        }

        public Dictionary<string, object> TransferMetadata { get; set; }
        public UploadSpecification UploadSpecification { get; protected set; }
        public EventHandler<TransferEventArgs> OnTransferProgress;

        protected int? ExpirationDays { get; set; }
        protected bool Prepared;

        protected readonly UploadSpecificationRequest UploadSpecificationRequest;
        protected readonly IPlatformFile File;

        public ShareFileClient Client { get; protected set; }
        public IMD5HashProvider HashProvider { get; protected set; }

        protected const int MaxBufferLength = 65536;

        protected virtual void NotifyProgress(TransferProgress progress)
        {
            if (OnTransferProgress != null)
            {
                OnTransferProgress.Invoke(this, new TransferEventArgs { Progress = progress });
            }
        }

        protected IQuery<UploadSpecification> CreateUploadSpecificationQuery(UploadSpecificationRequest uploadSpecificationRequest)
        {
            var query = Client.Items.Upload(uploadSpecificationRequest.Parent, uploadSpecificationRequest.Method,
                uploadSpecificationRequest.Raw, uploadSpecificationRequest.FileName, uploadSpecificationRequest.FileSize,
                uploadSpecificationRequest.BatchId,
                uploadSpecificationRequest.BatchLast, uploadSpecificationRequest.CanResume,
                uploadSpecificationRequest.StartOver, uploadSpecificationRequest.Unzip, uploadSpecificationRequest.Tool,
                uploadSpecificationRequest.Overwrite, uploadSpecificationRequest.Title,
                uploadSpecificationRequest.Details, uploadSpecificationRequest.IsSend,
                uploadSpecificationRequest.SendGuid, null, uploadSpecificationRequest.ThreadCount,
                uploadSpecificationRequest.ResponseFormat, uploadSpecificationRequest.Notify,
                uploadSpecificationRequest.ClientCreatedDateUtc, uploadSpecificationRequest.ClientModifiedDateUtc);

            return query;
        }

        protected HttpClientHandler GetHttpClientHandler()
        {
            var httpClientHandler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                CookieContainer = Client.CookieContainer,
                Credentials = Client.CredentialCache,
                Proxy = Client.Configuration.ProxyConfiguration
            };

            if (Client.Configuration.ProxyConfiguration != null && httpClientHandler.SupportsProxy)
            {
                httpClientHandler.UseProxy = true;
            }

            return httpClientHandler;
        }

        protected internal virtual HttpClient GetHttpClient()
        {
            return new HttpClient(GetHttpClientHandler());
        }

        /// <summary>
        /// Use specifically for Standard Uploads. The API call isn't guaranteed to include fmt=json on the query string
        /// this is necessary to get file metadata back as part of the upload response.
        /// </summary>
        /// <returns></returns>
        protected Uri GetChunkUriForStandardUploads()
        {
            var uploadUri = UploadSpecification.ChunkUri;

            // Only add fmt=json if it does not already exist, just in case there is an API update to correct this.
            if (UploadSpecification.ChunkUri.AbsoluteUri.IndexOf("&fmt=json", StringComparison.OrdinalIgnoreCase) == -1)
            {
                uploadUri = new Uri(UploadSpecification.ChunkUri.AbsoluteUri + "&fmt=json");
            }

            return uploadUri;
        }
    }
}
