using System;
using System.Collections.Generic;
using System.Net.Http;
using ShareFile.Api.Client.FileSystem;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Client.Transfers;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Core.Transfers.Uploaders
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
    }
}
