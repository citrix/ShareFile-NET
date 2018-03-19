using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;
using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    public abstract class UploaderBase : TransfererBase
    {
        protected UploaderBase(ShareFileClient client, UploadSpecificationRequest uploadSpecificationRequest, Stream stream, FileUploaderConfig config, int? expirationDays)
        {
            Client = client;
            UploadSpecificationRequest = uploadSpecificationRequest;
            FileStream = stream;
            Config = config ?? new FileUploaderConfig();
            if (uploadSpecificationRequest.FileSize != stream.Length)
            {
                throw new UploadException("Specified file size does not equal file stream length", UploadStatusCode.Unknown);
            }

            ExpirationDays = expirationDays;
            progressReporter = new TransferProgressReporter(uploadSpecificationRequest.FileSize, Guid.NewGuid().ToString(), Config.ProgressReportInterval);
        }

        public FileUploaderConfig Config { get; private set; }
        public abstract long LastConsecutiveByteUploaded { get; }
        public UploadSpecification UploadSpecification { get; protected set; }

        protected readonly TransferProgressReporter progressReporter;
        public event EventHandler<TransferEventArgs> OnTransferProgress
        {
            add { progressReporter.OnTransferProgress += value; }
            remove { progressReporter.OnTransferProgress -= value; }
        }
        
        protected int? ExpirationDays { get; set; }
        protected bool Prepared;
        protected readonly UploadSpecificationRequest UploadSpecificationRequest;
        protected readonly Stream FileStream;

        public ShareFileClient Client { get; private set; }
        public IMD5HashProvider HashProvider { get; protected set; }

        [Obsolete]
        public static int DefaultBufferLength => Configuration.BufferSize;

        protected IQuery<UploadSpecification> CreateUploadSpecificationQuery(UploadSpecificationRequest uploadSpecificationRequest)
        {
            if (uploadSpecificationRequest.ProviderCapabilities.SupportsUploadWithRequestParams())
            {
                return CreateUploadRequestParamsQuery(uploadSpecificationRequest);
            }
            var query = Client.Items.Upload(uploadSpecificationRequest.Parent,
                                            uploadSpecificationRequest.Method.GetValueOrDefault(UploadMethod.Threaded),
                                            uploadSpecificationRequest.Raw,
                                            uploadSpecificationRequest.FileName,
                                            uploadSpecificationRequest.FileSize,
                                            uploadSpecificationRequest.BatchId,
                                            uploadSpecificationRequest.BatchLast,
                                            uploadSpecificationRequest.CanResume,
                                            uploadSpecificationRequest.StartOver,
                                            uploadSpecificationRequest.Unzip,
                                            uploadSpecificationRequest.Tool,
                                            uploadSpecificationRequest.Overwrite,
                                            uploadSpecificationRequest.Title,
                                            uploadSpecificationRequest.Details,
                                            uploadSpecificationRequest.IsSend,
                                            uploadSpecificationRequest.SendGuid,
                                            null,
                                            uploadSpecificationRequest.ThreadCount,
                                            uploadSpecificationRequest.ResponseFormat,
                                            uploadSpecificationRequest.Notify,
                                            uploadSpecificationRequest.ClientCreatedDateUtc,
                                            uploadSpecificationRequest.ClientModifiedDateUtc,
                                            ExpirationDays,
                                            uploadSpecificationRequest.BaseFileId);

            return query;
        }

        protected IQuery<ODataFeed<Capability>> CreateCapabilitiesQuery()
        {
            return Client.Capabilities.Get()
                .WithBaseUri(UploadSpecificationRequest.Parent);
        }

        protected IQuery<UploadSpecification> CreateUploadRequestParamsQuery(
            UploadSpecificationRequest uploadSpecificationRequest)
        {
            var query = Client.Items.Upload2(uploadSpecificationRequest.Parent,
                uploadSpecificationRequest.ToRequestParams(), ExpirationDays);

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

        /// <summary>
        /// Used by some external tests
        /// </summary>
        /// <param name="progress"></param>
        protected virtual void NotifyProgress(TransferProgress progress)
        {
            progressReporter.ImmediatelyReportProgress(progress.BytesTransferred);
        }

        private HttpClient HttpClient { get; set; }
        protected virtual HttpClient GetHttpClient()
        {
            if (HttpClient == null)
            {
                if (Config.HttpClientFactory != null)
                {
                    HttpClient = Config.HttpClientFactory(Client.CredentialCache, Client.CookieContainer);
                }
                else
                {
                    HttpClient = new HttpClient(GetHttpClientHandler())
                    {
                        Timeout = new TimeSpan(0, 0, 0, 0, Config.HttpTimeout)
                    };
                }
            }
            return HttpClient;
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
            if (uploadUri.AbsoluteUri.IndexOf("&fmt=json", StringComparison.OrdinalIgnoreCase) == -1)
            {
                uploadUri = new Uri(uploadUri.AbsoluteUri + "&fmt=json");
            }
            if (UploadSpecificationRequest.ForceUnique)
            {
                uploadUri = new Uri(uploadUri.AbsoluteUri + "&forceunique=1");
            }

            return uploadUri;
        }

        protected Uri GetFinishUriForThreadedUploads()
        {
            var finishUri = new StringBuilder(string.Format("{0}&respformat=json", UploadSpecification.FinishUri.AbsoluteUri));

            if (UploadSpecificationRequest.FileSize > 0)
            {
                finishUri.AppendFormat("&filehash={0}", HashProvider.GetComputedHashAsString());
            }

            if (!string.IsNullOrEmpty(UploadSpecificationRequest.Details))
            {
                finishUri.AppendFormat("&details={0}", Uri.EscapeDataString(UploadSpecificationRequest.Details));
            }
            if (!string.IsNullOrEmpty(UploadSpecificationRequest.Title))
            {
                finishUri.AppendFormat("&title={0}", Uri.EscapeDataString(UploadSpecificationRequest.Title));
            }
            if (UploadSpecificationRequest.ForceUnique)
            {
                finishUri.Append("&forceunique=1");
            }

            return new Uri(finishUri.ToString());
        }

        /// <summary>
        /// Attempts to parse the chunk response. Throws if there was an upload error.
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <param name="responseContent"></param>
        /// <exception cref="UploadException" />
        protected void ValidateChunkResponse(HttpResponseMessage responseMessage, string responseContent)
        {
            ValidateStorageCenterResponse<string>(responseMessage, responseContent);
        }

        /// <summary>
        /// Attempts to parse the upload response. Throws if there was an upload error.
        /// </summary>
        /// <param name="responseMessage"></param>
        /// <param name="responseContent"></param>
        /// <param name="localHash">The locally computed file hash</param>
        /// <returns></returns>
        /// <exception cref="UploadException" />
        protected UploadResponse ValidateUploadResponse(HttpResponseMessage responseMessage, string responseContent, string localHash)
        {
            var response = ValidateStorageCenterResponse<UploadResponse>(responseMessage, responseContent);
            foreach (var upload in response)
            {
                upload.LocalHash = localHash;
            }

            return response;
        }

        private T ValidateStorageCenterResponse<T>(HttpResponseMessage responseMessage, string responseContent)
        {
            try
            {
                try
                {
                    var uploadResponse = JsonConvert.DeserializeObject<ShareFileApiResponse<T>>(responseContent);
                    if (uploadResponse.Error == null)
                    {
                        ParseODataExceptionAndThrow(responseContent);
                    }
                    else if (uploadResponse.Error.GetValueOrDefault())
                    {
                        throw new UploadException(uploadResponse.ErrorMessage, (UploadStatusCode)uploadResponse.ErrorCode);
                    }
                    else if (!responseMessage.IsSuccessStatusCode)
                    {
                        // response content is valid/success
                        throw new UploadException("StorageCenter error: " + responseMessage.StatusCode, UploadStatusCode.Unknown);
                    }
                    return uploadResponse.Value;
                }
                catch (JsonException jEx)
                {
                    ParseODataExceptionAndThrow(responseContent);

                    throw new UploadException("StorageCenter error: " + responseContent, UploadStatusCode.Unknown, jEx);
                }
            }
            catch (UploadException uploadEx)
            {
                if (!responseMessage.IsSuccessStatusCode)
                {
                    uploadEx.HttpStatusCode = responseMessage.StatusCode;
                }
                throw;
            }
        }

        /// <summary>
        /// Attempt to parse the input into an <see cref="ODataException"/> and wrap it in a <see cref="UploadException"/>.
        /// This will always throw an <see cref="UploadException"/>
        /// </summary>
        /// <param name="errorResponse">The json message to parse</param>
        /// <exception cref="UploadException" />
        private void ParseODataExceptionAndThrow(string errorResponse)
        {
            Client.Logging.Error(errorResponse);
            try
            {
                using (var textReader = new JsonTextReader(new StringReader(errorResponse)))
                {
                    var requestMessage = Client.Serializer.Deserialize<ODataRequestException>(textReader);
                    if (requestMessage.Message == null)
                    {
                        requestMessage.Message = new ODataExceptionMessage();
                    }
                    throw new UploadException(requestMessage.Message.Message, (UploadStatusCode)requestMessage.Code, new ODataException
                    {
                        Code = requestMessage.Code,
                        ODataExceptionMessage = requestMessage.Message,
                        ExceptionReason = requestMessage.ExceptionReason
                    });
                }
            }
            catch (JsonException jEx)
            {
                throw new UploadException("StorageCenter error: " + errorResponse, UploadStatusCode.Unknown, jEx);
            }
        }
    }
}