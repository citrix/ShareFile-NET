using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShareFile.Api.Client.Converters;
using ShareFile.Api.Client.Credentials;
using ShareFile.Api.Client.Entities;
using ShareFile.Api.Client.Events;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Requests.Executors;
using ShareFile.Api.Client.Requests.Providers;
using ShareFile.Api.Client.Security;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Client.Transfers;
using ShareFile.Api.Client.Transfers.Downloaders;
using ShareFile.Api.Client.Transfers.Uploaders;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client
{
    public partial interface IShareFileClient
    {
        Uri BaseUri { get; set; }
        Configuration Configuration { get; set; }

        AsyncUploaderBase GetAsyncFileUploader(
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream, 
            FileUploaderConfig config = null,
            int? expirationDays = null);
        
        AsyncUploaderBase GetAsyncFileUploader(
            ActiveUploadState activeUploadState,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null);

        AsyncFileDownloader GetAsyncFileDownloader(Item itemToDownload, DownloaderConfig config = null);

        AsyncFileDownloader GetAsyncFileDownloader(DownloadSpecification downloadSpecification, DownloaderConfig config = null);

        void RegisterAsyncRequestExecutor(IAsyncRequestExecutor asyncRequestExecutor);

        SyncUploaderBase GetFileUploader(
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null,
            int? expirationDays = null);
        
        SyncUploaderBase GetFileUploader(
            ActiveUploadState activeUploadState,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null);

        FileDownloader GetFileDownloader(Item itemToDownload, DownloaderConfig config = null);

        FileDownloader GetFileDownloader(DownloadSpecification downloadSpecification, DownloaderConfig config = null);

        void RegisterSyncRequestExecutor(ISyncRequestExecutor syncRequestExecutor);

        void AddCookie(Uri host, Cookie cookie);

        /// <summary>
        /// Use this method if you've previously acquired an AuthenticationId through other means.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="authenticationId"></param>
        /// <param name="path"></param>
        /// <param name="cookieName"></param>
        void AddAuthenticationId(Uri host, string authenticationId, string path = "", string cookieName = "SFAPI_AuthId");

        void AddChangeDomainHandler(ChangeDomainCallback handler);
        void AddExceptionHandler(ExceptionCallback handler);
        bool RemoveChangeDomainHandler(ChangeDomainCallback handler);
        bool RemoveExceptionHandler(ExceptionCallback handler);

        EventHandlerResponse OnException(HttpResponseMessage responseMessage, int retryCount);
        EventHandlerResponse OnChangeDomain(HttpRequestMessage requestMessage, Redirection redirection);

        void RegisterSyncRequestProvider(ISyncRequestProvider syncRequestProvider);
        void RegisterAsyncRequestProvider(IAsyncRequestProvider asyncRequestProvider);
        void RegisterSyncRequestProvider(Func<ISyncRequestProvider> syncRequestProvider);
        void RegisterAsyncRequestProvider(Func<IAsyncRequestProvider> asyncRequestProvider);

        bool HasCredentials(Uri uri, string authenticationType = "");
        NetworkCredential GetCredential(Uri uri, string authenticationType = "");

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="authenticationType"></param>
        /// <param name="networkCredential"></param>
        void AddCredentials(Uri uri, string authenticationType, NetworkCredential networkCredential);

        /// <summary>
        /// </summary>
        /// <param name="host"></param>
        /// <param name="oauthToken"></param>
        void AddOAuthCredentials(Uri host, string oauthToken);

        /// <summary>
        /// </summary>
        /// <param name="oauthToken"></param>
        void AddOAuthCredentials(OAuthToken oauthToken);

        void RemoveCredentials(Uri uri, string authenticationType);

        void ClearCredentialsAndCookies();

        /// <summary>
        /// Substitute TNew for TReplace when instantiating TReplace for responses.
        /// Helpful if you need some additional properties on responses to work with.
        /// </summary>
        /// <typeparam name="TNew"></typeparam>
        /// <typeparam name="TReplace"></typeparam>
        void RegisterType<TNew, TReplace>()
            where TNew : TReplace
            where TReplace : ODataObject;

        T Entities<T>() where T : EntityBase;

        [NotNull]
        Task<Stream> ExecuteAsync(IQuery<Stream> stream, CancellationToken token = default(CancellationToken));
        [NotNull]
        Task<Stream> ExecuteAsync(IStreamQuery stream, CancellationToken token = default(CancellationToken));

        [NotNull]
        Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken token = default(CancellationToken))
            where T : class;

        [NotNull]
        Task ExecuteAsync(IQuery query, CancellationToken token = default(CancellationToken));

        Stream Execute(IQuery<Stream> stream);
        Stream Execute(IStreamQuery stream);

        T Execute<T>(IQuery<T> query)
            where T : class;
        void Execute(IQuery query);

        IEnumerable<Capability> GetCachedCapabilities(Uri itemUri);
        void SetCachedCapabilities(Uri itemUri, IEnumerable<Capability> capabilities);

        void SetConfiguration(Configuration configuration);
    }

    public partial class ShareFileClient : IShareFileClient
    {
        private static readonly Dictionary<string, EntityBase> RegisteredEntities;
        static ShareFileClient()
        {
            RegisteredEntities = new Dictionary<string, EntityBase>();
        }

        public ShareFileClient(string baseUri, Configuration configuration = null)
            : this()
        {
            BaseUri = new Uri(baseUri);

            Configuration = configuration ?? Configuration.Default();
            CookieContainer = new CookieContainer();
            Logging = new LoggingProvider(Configuration.Logger);

            CredentialCache = CredentialCacheFactory.GetCredentialCache();
            Serializer = GetSerializer();
            LoggingSerializer = GetLoggingSerializer(this);
            _capabilityCache = new Dictionary<string, IEnumerable<Capability>>();

            RegisterRequestProviders();
            InitializeHttpClient();
        }

        public Uri BaseUri { get; set; }

        public Configuration Configuration { get; set; }

        internal HttpClient HttpClient { get; set; }
        internal LoggingProvider Logging { get; set; }
        protected internal ICredentialCache CredentialCache { get; set; }
        protected internal CookieContainer CookieContainer { get; set; }
        protected internal JsonSerializer Serializer { get; set; }
        protected internal JsonSerializer LoggingSerializer { get; set; }
        internal RequestProviderFactory RequestProviderFactory { get; set; }

        internal void RegisterRequestProviders()
        {
            RequestProviderFactory = new RequestProviderFactory();

            RegisterSyncRequestProvider(() => new SyncRequestProvider(this));
            if (RequestExecutorFactory.GetSyncRequestExecutor() == null)
            {
                RequestExecutorFactory.RegisterSyncRequestProvider(new SyncRequestExecutor());
            }
#if ASYNC
            RegisterAsyncRequestProvider(() => new AsyncRequestProvider(this));
            if (RequestExecutorFactory.GetAsyncRequestExecutor() == null)
            {
                RequestExecutorFactory.RegisterAsyncRequestProvider(new AsyncRequestExecutor());
            }
#endif
        }

        internal void InitializeHttpClient()
        {
            HttpClient = Configuration.HttpClientFactory != null 
                ? Configuration.HttpClientFactory(CredentialCache, CookieContainer) 
                : CreateHttpClient();
        }
        private HttpClientHandler CreateHttpHandler()
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                Credentials = CredentialCache
            };

            if (handler.SupportsAutomaticDecompression)
            {
                handler.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            }

            if (!BaseRequestProvider.RuntimeRequiresCustomCookieHandling)
            {
                handler.UseCookies = true;
                handler.CookieContainer = CookieContainer;
            }
            else
            {
                handler.UseCookies = false;
            }

            // Not all platforms support proxy.
            if (Configuration.ProxyConfiguration != null && handler.SupportsProxy)
            {
                handler.Proxy = Configuration.ProxyConfiguration;
                handler.UseProxy = true;
            }

            return handler;
        }
        private HttpClient CreateHttpClient()
        {
            var handler = CreateHttpHandler();
            return new HttpClient(handler, false)
            {
                Timeout = new TimeSpan(0, 0, 0, 0, Configuration.HttpTimeout)
            };
        }

        private static JsonSerializer GetSerializer()
        {
            return new JsonSerializer
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                Converters = { new ODataConverter(), new StringEnumConverter(), new SafeEnumConverter() }
            };
        }

        private static JsonSerializer GetLoggingSerializer(ShareFileClient client)
        {
            return new JsonSerializer
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new LoggingConverter(client), new StringEnumConverter(), new SafeEnumConverter() }
            };
        }

        private const int StandardUploadThreshold = 1024 * 1024 * 8;
        private UploadMethod GetUploadMethod(long fileSize)
        {
            if (fileSize > StandardUploadThreshold)
            {
                return UploadMethod.Threaded;
            }
            return UploadMethod.Standard;
        }

        private Dictionary<string, IEnumerable<Capability>> _capabilityCache;

        public IEnumerable<Capability> GetCachedCapabilities(Uri itemUri)
        {
            if (itemUri == null) return null;

            IEnumerable<Capability> capabilities = null;
            _capabilityCache.TryGetValue(GetCapabilityCacheKey(itemUri), out capabilities);
            return capabilities;
        }

        public void SetCachedCapabilities(Uri itemUri, IEnumerable<Capability> capabilities)
        {
            if (itemUri == null) return;

            if (!_capabilityCache.ContainsKey(GetCapabilityCacheKey(itemUri)))
            {
                lock (_capabilityCache)
                {
                    _capabilityCache[GetCapabilityCacheKey(itemUri)] = capabilities;
                }
            }
        }

        protected string GetCapabilityCacheKey(Uri itemUri)
        {
            return String.Format("{0}/{1}", itemUri.Host.ToLower(), GetProvider(itemUri).ToLower());
        }


        /// <summary>
        /// Use some naive metrics for deciding which <see cref="UploadMethod"/>  should be used.
        /// </summary>
        /// <param name="uploadSpecificationRequest"></param>
        private void PreprocessUploadSpecRequest(UploadSpecificationRequest uploadSpecificationRequest)
        {
            if (string.IsNullOrEmpty(uploadSpecificationRequest.Tool))
            {
                uploadSpecificationRequest.Tool = Configuration.ToolName;
            }

            if (uploadSpecificationRequest.Method.HasValue) return;


            if (uploadSpecificationRequest.ProviderCapabilities == null)
            {
                uploadSpecificationRequest.ProviderCapabilities = GetCachedCapabilities(uploadSpecificationRequest.Parent);
            }

            if (uploadSpecificationRequest.ProviderCapabilities != null && uploadSpecificationRequest.Method == null)
            {
                if (uploadSpecificationRequest.ProviderCapabilities.Any(x => x.Name == CapabilityName.StandardUploadRaw))
                {
                    uploadSpecificationRequest.Method = this.GetUploadMethod(uploadSpecificationRequest.FileSize);
                }
                else if (uploadSpecificationRequest.ProviderCapabilities.Any(x => x.Name == CapabilityName.ThreadedUploadRaw))
                {
                    uploadSpecificationRequest.Method = UploadMethod.Threaded;
                }
                else
                {
                    // Bug - 4.4.2016 - SFSZP-451 - RZ doesn't return upload capabilities so default to this even it it might not work
                    uploadSpecificationRequest.Method = this.GetUploadMethod(uploadSpecificationRequest.FileSize);
                }
            }
            else
            {
                uploadSpecificationRequest.Method = UploadMethod.Threaded;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadSpecificationRequest"></param>
        /// <param name="file"></param>
        /// <param name="config"></param>
        /// <param name="expirationDays">Will only be obeyed by ShareFile apps</param>
        /// <returns></returns>
        public AsyncUploaderBase GetAsyncFileUploader(
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null,
            int? expirationDays = null)
        {
            this.PreprocessUploadSpecRequest(uploadSpecificationRequest);

            switch (uploadSpecificationRequest.Method)
            {
                case UploadMethod.Standard:
                    return new AsyncStandardFileUploader(this, uploadSpecificationRequest, stream, config, expirationDays);
                case UploadMethod.Threaded:
                    return new AsyncScalingFileUploader(this, uploadSpecificationRequest, stream, config, expirationDays);
            }

            throw new NotSupportedException(uploadSpecificationRequest.Method + " is not supported.");
        }

        public AsyncUploaderBase GetAsyncFileUploader(
            ActiveUploadState activeUploadState,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null)
        {
            this.PreprocessUploadSpecRequest(uploadSpecificationRequest);

            switch (uploadSpecificationRequest.Method)
            {
                case UploadMethod.Standard:
                    return new AsyncStandardFileUploader(this, uploadSpecificationRequest, stream, config);
                case UploadMethod.Threaded:
                    return new AsyncScalingFileUploader(
                        this,
                        activeUploadState,
                        uploadSpecificationRequest,
                        stream,
                        config);
            }

            throw new NotSupportedException(uploadSpecificationRequest.Method + " is not supported.");
        }
        
        public AsyncFileDownloader GetAsyncFileDownloader(Item itemToDownload, DownloaderConfig config = null)
        {
            return new AsyncFileDownloader(itemToDownload, this, config);
        }

        public AsyncFileDownloader GetAsyncFileDownloader(DownloadSpecification downloadSpecification, DownloaderConfig config = null)
        {
            return new AsyncFileDownloader(downloadSpecification, this, config);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadSpecificationRequest"></param>
        /// <param name="file"></param>
        /// <param name="config"></param>
        /// <param name="expirationDays">Will only be obeyed by ShareFile apps</param>
        /// <returns></returns>
        public SyncUploaderBase GetFileUploader(
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null,
            int? expirationDays = null)
        {
            this.PreprocessUploadSpecRequest(uploadSpecificationRequest);

            switch (uploadSpecificationRequest.Method)
            {
                case UploadMethod.Standard:
                    return new StandardFileUploader(this, uploadSpecificationRequest, stream, config, expirationDays);
                case UploadMethod.Threaded:
                    return new ScalingFileUploader(this, uploadSpecificationRequest, stream, config, expirationDays);
            }

            throw new NotSupportedException(uploadSpecificationRequest.Method + " is not supported.");
        }

        public SyncUploaderBase GetFileUploader(
            ActiveUploadState activeUploadState,
            UploadSpecificationRequest uploadSpecificationRequest,
            Stream stream,
            FileUploaderConfig config = null)
        {
            UploadMethod? method = activeUploadState.UploadSpecification.Method;
            if (!method.HasValue)
                throw new ArgumentNullException("UploadSpecification.Method");
            switch (method.Value)
            {
                case UploadMethod.Standard:
                    return new StandardFileUploader(this, activeUploadState.UploadSpecification, uploadSpecificationRequest, stream, config);
                case UploadMethod.Threaded:
                    return new ScalingFileUploader(this, activeUploadState, uploadSpecificationRequest, stream, config);
                default:
                    throw new NotSupportedException($"{method} is not supported");
            }
        }
        
        public FileDownloader GetFileDownloader(Item itemToDownload, DownloaderConfig config = null)
        {
            return new FileDownloader(itemToDownload, this, config);
        }

        public FileDownloader GetFileDownloader(DownloadSpecification downloadSpecification, DownloaderConfig config = null)
        {
            return new FileDownloader(downloadSpecification, this, config);
        }

        public void AddCookie(Uri host, Cookie cookie)
        {
            Logging.Info("Add cookie");
            Logging.Debug("Cookie: {1} for {0}", new object[] { host.ToString(), cookie.ToString() });

            CookieContainer.Add(host, cookie);
        }

        /// <summary>
        /// Use this method if you've previously acquired an AuthenticationId through other means.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="authenticationId"></param>
        /// <param name="path"></param>
        /// <param name="cookieName"></param>
        public void AddAuthenticationId(Uri host, string authenticationId, string path = "", string cookieName = "SFAPI_AuthId")
        {
            Logging.Info("Adding AuthenticationId");
            Logging.Debug("Host: {0}; CookieName: {1}; CookiePath: {2}", new object[] { host, cookieName, path });

            CookieContainer.Add(host, new Cookie(cookieName, authenticationId, path));
        }

        public void ClearCredentialsAndCookies()
        {
            CredentialCache = new ShareFile.Api.Client.Credentials.CredentialCache();
            CookieContainer = new CookieContainer();
            InitializeHttpClient();
        }

        /// <summary>
        /// Substitute TNew for TReplace when instantiating TReplace for responses.
        /// </summary>
        /// <typeparam name="TNew"></typeparam>
        /// <typeparam name="TReplace"></typeparam>
        public virtual void RegisterType<TNew, TReplace>()
            where TNew : TReplace
            where TReplace : ODataObject
        {
            ODataFactory.GetInstance().RegisterType<TNew, TReplace>();
        }

        protected readonly List<ChangeDomainCallback> ChangeDomainHandlers = new List<ChangeDomainCallback>(capacity: 0);

        protected readonly List<ExceptionCallback> ExceptionHandlers = new List<ExceptionCallback>(capacity: 0);

        public void AddChangeDomainHandler(ChangeDomainCallback handler)
        {
            lock (ChangeDomainHandlers)
            {
                ChangeDomainHandlers.Add(handler);
            }
        }

        public void AddExceptionHandler(ExceptionCallback handler)
        {
            lock (ExceptionHandlers)
            {
                ExceptionHandlers.Add(handler);
            }
        }

        public bool RemoveChangeDomainHandler(ChangeDomainCallback handler)
        {
            lock(ChangeDomainHandlers)
            {
                return ChangeDomainHandlers.Remove(handler);
            }
        }

        public bool RemoveExceptionHandler(ExceptionCallback handler)
        {
            lock (ExceptionHandlers)
            {
                return ExceptionHandlers.Remove(handler);
            }
        }

        public EventHandlerResponse OnException(HttpResponseMessage responseMessage, int retryCount)
        {
            List<ExceptionCallback> handlers;
            lock(ExceptionHandlers)
            {
                handlers = new List<ExceptionCallback>(ExceptionHandlers);
            }
            foreach (var handler in handlers)
            {
                var action = handler(responseMessage, retryCount);
                if (action.Action != EventHandlerResponseAction.Ignore)
                {
                    return action;
                }
            }
            return EventHandlerResponse.Throw;
        }

        public EventHandlerResponse OnChangeDomain(HttpRequestMessage requestMessage, Redirection redirection)
        {
            List<ChangeDomainCallback> handlers;
            lock(ChangeDomainHandlers)
            {
                handlers = new List<ChangeDomainCallback>(ChangeDomainHandlers);
            }
            foreach (var handler in handlers)
            {
                var action = handler(requestMessage, redirection);
                if (action.Action != EventHandlerResponseAction.Ignore)
                {
                    return action;
                }
            }
            return new EventHandlerResponse() { Action = EventHandlerResponseAction.Redirect, Redirection = redirection };
        }

        public void RegisterSyncRequestProvider(ISyncRequestProvider syncRequestProvider)
        {
            RequestProviderFactory.RegisterSyncRequestProvider(() => syncRequestProvider);
        }

        public void RegisterAsyncRequestProvider(IAsyncRequestProvider asyncRequestProvider)
        {
            RequestProviderFactory.RegisterAsyncRequestProvider(() => asyncRequestProvider);
        }

        public void RegisterSyncRequestProvider(Func<ISyncRequestProvider> syncRequestProvider)
        {
            RequestProviderFactory.RegisterSyncRequestProvider(syncRequestProvider);
        }

        public void RegisterAsyncRequestProvider(Func<IAsyncRequestProvider> asyncRequestProvider)
        {
            RequestProviderFactory.RegisterAsyncRequestProvider(asyncRequestProvider);
        }

        protected internal ISyncRequestExecutor SyncRequestExecutor { get; set; }

        protected internal IAsyncRequestExecutor AsyncRequestExecutor { get; set; }

        /// <summary>
        /// This RequestExecutor will be used for all requests created by this ShareFileClient instance, overriding the global RequestExecutorFactory.
        /// </summary>
        /// <param name="syncRequestExecutor"></param>
        public void RegisterSyncRequestExecutor(ISyncRequestExecutor syncRequestExecutor)
        {
            SyncRequestExecutor = syncRequestExecutor;
        }

        /// <summary>
        /// This RequestExecutor will be used for all requests created by this ShareFileClient instance, overriding the global RequestExecutorFactory. 
        /// </summary>
        /// <param name="asyncRequestExecutor"></param>
        public void RegisterAsyncRequestExecutor(IAsyncRequestExecutor asyncRequestExecutor)
        {
            AsyncRequestExecutor = asyncRequestExecutor;
        }

        public bool HasCredentials(Uri uri, string authenticationType = "")
        {
            return IsValidCredential(GetCredential(uri, authenticationType));
        }

        public NetworkCredential GetCredential(Uri uri, string authenticationType = "")
        {
            var existingCredential = CredentialCache.GetCredential(uri, authenticationType);

            if (IsValidCredential(existingCredential))
            {
                return existingCredential;
            }
            return null;
        }

        protected bool IsValidCredential(NetworkCredential credential)
        {
            return credential != null && credential != CredentialAuthorityContainer.Default.Credentials;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="authenticationType"></param>
        /// <param name="networkCredential"></param>
        public void AddCredentials(Uri uri, string authenticationType, NetworkCredential networkCredential)
        {
            var existingCredential = CredentialCache.GetCredential(uri, authenticationType);

            if (existingCredential != null)
            {
                CredentialCache.Remove(uri, authenticationType);
            }
            CredentialCache.Add(uri, authenticationType, networkCredential);
        }

        /// <summary>
        /// </summary>
        /// <param name="host"></param>
        /// <param name="oauthToken"></param>
        public void AddOAuthCredentials(Uri host, string oauthToken)
        {
            Logging.Info("Adding OAuth Credentials");
            Logging.Debug("Host: {0}", new object[] { host });

            try
            {
                var existingCredentials = CredentialCache.GetCredential(host, "Bearer");
                if (existingCredentials != null)
                {
                    CredentialCache.Remove(host, "Bearer");
                }
            }
            catch (Exception exception)
            {
                Logging.Error(exception, "Failed to add OAuth credentials");
            }
            finally
            {
                CredentialCache.Add(host, "Bearer", new OAuth2Credential(oauthToken));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="oauthToken"></param>
        public void AddOAuthCredentials(OAuthToken oauthToken)
        {
            var host = oauthToken.GetUri();

            Logging.Info("Adding OAuth Credentials using oauthToken");
            Logging.Debug("Host: {0}", new object[] { host });

            try
            {
                var existingCredentials = CredentialCache.GetCredential(host, "Bearer");
                if (existingCredentials != null)
                {
                    CredentialCache.Remove(host, "Bearer");
                }
            }
            catch (Exception exception)
            {
                Logging.Error(exception, "Failed to add OAuth credentials");
            }
            finally
            {
                CredentialCache.Add(host, "Bearer", new OAuth2Credential(oauthToken));
            }
        }

        public void RemoveCredentials(Uri uri, string authenticationType)
        {
            CredentialCache.Remove(uri, authenticationType);
        }

        #region Entity Registration

        public T Entities<T>() where T : EntityBase
        {
            EntityBase entity;
            if (!RegisteredEntities.TryGetValue(typeof(T).FullName, out entity))
            {
                entity = (T)Activator.CreateInstance(typeof(T), new object[] { this });

                RegisteredEntities.Add(typeof(T).FullName, entity);
            }
            return (T)entity;
        }

        #endregion

        [NotNull]
        public virtual Task<Stream> ExecuteAsync(IStreamQuery stream, CancellationToken token = default(CancellationToken))
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(stream, token);
        }

        [NotNull]
        public virtual Task<Stream> ExecuteAsync(IQuery<Stream> stream, CancellationToken token = default(CancellationToken))
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(stream, token);
        }

        [NotNull]
        public virtual Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken token = default(CancellationToken))
            where T : class
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(query, token);
        }

        [NotNull]
        public virtual Task ExecuteAsync(IQuery query, CancellationToken token = default(CancellationToken))
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(query, token);
        }

        public virtual Stream Execute(IStreamQuery stream)
        {
            return RequestProviderFactory.GetSyncRequestProvider().Execute(stream);
        }

        public virtual Stream Execute(IQuery<Stream> stream)
        {
            return RequestProviderFactory.GetSyncRequestProvider().Execute(stream);
        }

        public virtual T Execute<T>(IQuery<T> query)
            where T : class
        {
            return RequestProviderFactory.GetSyncRequestProvider().Execute(query);
        }

        public virtual void Execute(IQuery query)
        {
            RequestProviderFactory.GetSyncRequestProvider().Execute(query);
        }

        public static string GetProvider(Uri uri)
        {
            var path = uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (path.Length > 0)
                return path[0];
            return "sf";
        }

        public void SetConfiguration(Configuration configuration)
        {
            Configuration = configuration ?? Configuration.Default();
            Logging = new LoggingProvider(Configuration.Logger);
            InitializeHttpClient();
        }
    }
}