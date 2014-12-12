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
using ShareFile.Api.Client.FileSystem;
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
using ShareFile.Api.Models;

namespace ShareFile.Api.Client
{
    public partial interface IShareFileClient
    {
        Uri BaseUri { get; set; }
        Configuration Configuration { get; set; }

#if ShareFile
        ZoneAuthentication ZoneAuthentication { get; set; }
#endif

#if Async
        AsyncUploaderBase GetAsyncFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null);

        AsyncFileDownloader GetAsyncFileDownloader(Item itemToDownload, DownloaderConfig config = null);
#else
        SyncUploaderBase GetFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null);

        FileDownloader GetFileDownloader(Item itemToDownload, DownloaderConfig config = null);
#endif
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
        void RegisterAsyncRequestProvider(IAsyncRequestProvider syncRequestProvider);

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
#if Async
        Task<Stream> ExecuteAsync(IQuery<Stream> stream, CancellationToken? token = null);
        Task<Stream> ExecuteAsync(IStreamQuery stream, CancellationToken? token = null);

        Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken? token = null)
            where T : class;

        Task ExecuteAsync(IQuery query, CancellationToken? token = null);
#endif

        Stream Execute(IQuery<Stream> stream);
        Stream Execute(IStreamQuery stream);

        T Execute<T>(IQuery<T> query)
            where T : class;
        void Execute(IQuery query);
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

            RegisterRequestProviders();
        }

        public Uri BaseUri { get; set; }

        public Configuration Configuration { get; set; }

        internal LoggingProvider Logging { get; set; }
        internal ICredentialCache CredentialCache { get; set; }
        internal CookieContainer CookieContainer { get; set; }
        internal JsonSerializer Serializer { get; set; }
        internal JsonSerializer LoggingSerializer { get; set; }
        internal RequestProviderFactory RequestProviderFactory { get; set; }

#if ShareFile
        private ZoneAuthentication _zoneAuthentication;
        public ZoneAuthentication ZoneAuthentication
        {
            get { return _zoneAuthentication; }
            set
            {
                if (!HmacSha256ProviderFactory.HasProvider())
                {
                    throw new Exception("A IHmacSha256Provider has not been registered, this is required for ZoneAuthentication to sign requests.");
                }
                _zoneAuthentication = value;
            }
        }
#endif

        internal void RegisterRequestProviders()
        {
            RequestProviderFactory = new RequestProviderFactory();

            RegisterSyncRequestProvider(new SyncRequestProvider(this));
            RequestExecutorFactory.RegisterSyncRequestProvider(new SyncRequestExecutor());
#if Async
            RegisterAsyncRequestProvider(new AsyncRequestProvider(this));
            RequestExecutorFactory.RegisterAsyncRequestProvider(new AsyncRequestExecutor());
#endif
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
                Converters = {new LoggingConverter(client), new StringEnumConverter(), new SafeEnumConverter()}
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

        /// <summary>
        /// Use some naive metrics for deciding which <see cref="UploadMethod"/>  should be used.
        /// </summary>
        /// <param name="uploadSpecificationRequest"></param>
        private void SetUploadMethod(UploadSpecificationRequest uploadSpecificationRequest)
        {
            if (uploadSpecificationRequest.Method.HasValue) return;

            uploadSpecificationRequest.Method = this.GetUploadMethod(uploadSpecificationRequest.FileSize);
        }

#if Async
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadSpecificationRequest"></param>
        /// <param name="file"></param>
        /// <param name="config"></param>
        /// <param name="expirationDays">Will only be obeyed by ShareFile apps</param>
        /// <returns></returns>
        public AsyncUploaderBase GetAsyncFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
        {
            this.SetUploadMethod(uploadSpecificationRequest);

            switch (uploadSpecificationRequest.Method)
            {
                case UploadMethod.Standard:
                    return new AsyncStandardFileUploader(this, uploadSpecificationRequest, file, config, expirationDays);
                    break;
                case UploadMethod.Threaded:
                    return new AsyncScalingFileUploader(this, uploadSpecificationRequest, file, config, expirationDays);
                    break;
            }

            throw new NotSupportedException(uploadSpecificationRequest.Method + " is not supported.");
        }

        public AsyncFileDownloader GetAsyncFileDownloader(Item itemToDownload, DownloaderConfig config = null)
        {
            return new AsyncFileDownloader(itemToDownload, this, config);
        }
#else
        /// <summary>
        /// 
        /// </summary>
        /// <param name="uploadSpecificationRequest"></param>
        /// <param name="file"></param>
        /// <param name="config"></param>
        /// <param name="expirationDays">Will only be obeyed by ShareFile apps</param>
        /// <returns></returns>
        public SyncUploaderBase GetFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
        {
            this.SetUploadMethod(uploadSpecificationRequest);

            switch (uploadSpecificationRequest.Method)
            {
                case UploadMethod.Standard:
                    return new StandardFileUploader(this, uploadSpecificationRequest, file, config, expirationDays);
                    break;
                case UploadMethod.Threaded:
                    return new ScalingFileUploader(this, uploadSpecificationRequest, file, config, expirationDays);
                    break;
            }

            throw new NotSupportedException(uploadSpecificationRequest.Method + " is not supported.");
        }

        public FileDownloader GetFileDownloader(Item itemToDownload, DownloaderConfig config = null)
        {
            return new FileDownloader(itemToDownload, this, config);
        }

#endif
        public void AddCookie(Uri host, Cookie cookie)
        {
            Logging.Info("Add cookie");
            Logging.Debug("Cookie: {1} for {0}", host.ToString(), cookie.ToString());

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
            Logging.Debug("Host: {0}; CookieName: {1}; CookiePath: {2}", host, cookieName, path);

            CookieContainer.Add(host, new Cookie(cookieName, authenticationId, path));
        }

        public void ClearCredentialsAndCookies()
        {
            CredentialCache = new ShareFile.Api.Client.Credentials.CredentialCache();
            CookieContainer = new CookieContainer();
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

        protected List<ChangeDomainCallback> ChangeDomainHandlers { get; set; }
        protected List<ExceptionCallback> ExceptionHandlers { get; set; }

        public void AddChangeDomainHandler(ChangeDomainCallback handler)
        {
            if (ChangeDomainHandlers == null)
            {
                ChangeDomainHandlers = new List<ChangeDomainCallback>();
            }

            ChangeDomainHandlers.Add(handler);
        }

        public void AddExceptionHandler(ExceptionCallback handler)
        {
            if (ExceptionHandlers == null)
            {
                ExceptionHandlers = new List<ExceptionCallback>();
            }

            ExceptionHandlers.Add(handler);
        }

        public bool RemoveChangeDomainHandler(ChangeDomainCallback handler)
        {
            if (ChangeDomainHandlers == null) return false;

            return ChangeDomainHandlers.Remove(handler);
        }

        public bool RemoveExceptionHandler(ExceptionCallback handler)
        {
            if (ExceptionHandlers == null) return false;

            return ExceptionHandlers.Remove(handler);
        }

        public EventHandlerResponse OnException(HttpResponseMessage responseMessage, int retryCount)
        {
            if(ExceptionHandlers != null)
            {
                foreach (var handler in ExceptionHandlers)
                {
                    var action = handler(responseMessage, retryCount);
                    if (action.Action != EventHandlerResponseAction.Ignore) return action;
                }
            }
            return EventHandlerResponse.Throw;
        }

        public EventHandlerResponse OnChangeDomain(HttpRequestMessage requestMessage, Redirection redirection)
        {
            if (ChangeDomainHandlers != null)
            {
                foreach (var handler in ChangeDomainHandlers)
                {
                    var action = handler(requestMessage, redirection);
                    if (action.Action != EventHandlerResponseAction.Ignore) return action;
                }
            }
            return new EventHandlerResponse() { Action = EventHandlerResponseAction.Redirect, Redirection = redirection };
        }

        public void RegisterSyncRequestProvider(ISyncRequestProvider syncRequestProvider)
        {
            RequestProviderFactory.RegisterSyncRequestProvider(syncRequestProvider);
        }

        public void RegisterAsyncRequestProvider(IAsyncRequestProvider asyncRequestProvider)
        {
            RequestProviderFactory.RegisterAsyncRequestProvider(asyncRequestProvider);
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
            Logging.Debug("Host: {0}", host);

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
                Logging.Error("Failed to add OAuth credentials", exception);
            }
            finally
            {
                CredentialCache.Add(host, "Bearer", new OAuth2Credential(oauthToken));
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="host"></param>
        /// <param name="oauthToken"></param>
        public void AddOAuthCredentials(OAuthToken oauthToken)
        {
            var host = oauthToken.GetUri();

            Logging.Info("Adding OAuth Credentials using oauthToken");
            Logging.Debug("Host: {0}", host);

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
                Logging.Error("Failed to add OAuth credentials", exception);
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
            if (!RegisteredEntities.TryGetValue(typeof (T).FullName, out entity))
            {
                entity = (T)Activator.CreateInstance(typeof (T), new object[] {this});

                RegisteredEntities.Add(typeof(T).FullName, entity);
            }
            return (T)entity;
        }

        #endregion

#if Async
        public virtual Task<Stream> ExecuteAsync(IStreamQuery stream, CancellationToken? token = null)
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(stream, token);
        }

        public virtual Task<Stream> ExecuteAsync(IQuery<Stream> stream, CancellationToken? token = null)
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(stream, token);
        }

        public virtual Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken? token = null)
            where T : class
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(query, token);
        }

        public virtual Task ExecuteAsync(IQuery query, CancellationToken? token = null)
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(query, token);
        }
#endif

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
    }
}
