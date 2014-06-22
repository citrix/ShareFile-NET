using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    public interface IShareFileClient
    {
#if ShareFile
        IAccountsEntityInternal Accounts { get; }
        IDevicesEntityInternal Devices { get; }
        IItemsEntityInternal Items { get; }
        IStorageCentersEntityInternal StorageCenters { get; }
        IZonesEntityInternal Zones { get; }
#else
        IAccountsEntity Accounts { get; }
        IItemsEntity Items { get; }
#endif

        IAccessControlsEntity AccessControls { get; }
        IAsyncOperationsEntity AsyncOperations { get; }
        ICapabilitiesEntity Capabilities { get; }
        IConnectorGroupsEntity ConnectorGroups { get; }
        IConfigsEntity Configs { get; }
        IFavoriteFoldersEntity FavoriteFolders { get; }
        IGroupsEntity Groups { get; }
        IMetadataEntity Metadata { get; }
        ISessionsEntity Sessions { get; }
        ISharesEntity Shares { get; }
        IUsersEntity Users { get; }
        Uri BaseUri { get; set; }
        Configuration Configuration { get; set; }

#if ShareFile
        ZoneAuthentication ZoneAuthentication { get; set; }
#endif

#if Async
        AsyncThreadedFileUploader GetAsyncFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null);
        AsyncFileDownloader GetAsyncFileDownloader(Item itemToDownload, DownloaderConfig config = null);
#else
#if ShareFile
        ThreadedFileUploader GetFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null);
#else
        ThreadedFileUploader GetFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null);
#endif
        FileDownloader GetFileDownloader(Item itemToDownload, DownloaderConfig config = null);
#endif
        void AddCookie(Uri host, Cookie cookie);

#if ShareFile
        /// <summary>
        /// Use this method if you've previously acquired an AuthenticationId through other means.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="authenticationId"></param>
        /// <param name="path"></param>
        /// <param name="cookieName"></param>
        void AddAuthenticationId(Uri host, string authenticationId, string path = "", string cookieName = "SFAPI_AuthId");
#endif

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
        /// </summary>
        /// <typeparam name="TNew"></typeparam>
        /// <typeparam name="TReplace"></typeparam>
        void RegisterType<TNew, TReplace>()
            where TNew : TReplace
            where TReplace : ODataObject;

        T Entities<T>() where T : EntityBase;
        Task<Stream> ExecuteAsync(IStreamQuery stream, CancellationToken? token = null);
        Stream Execute(IStreamQuery stream);

        Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken? token = null)
            where T : class;

        T Execute<T>(IQuery<T> query)
            where T : class;

        Task ExecuteAsync(IQuery query, CancellationToken? token = null);
        void Execute(IQuery query);
    }

    public class ShareFileClient : IShareFileClient
    {
        private static readonly Dictionary<string, EntityBase> RegisteredEntities;
        static ShareFileClient()
        {
            RegisteredEntities = new Dictionary<string, EntityBase>();
        }

        public ShareFileClient(string baseUri, Configuration configuration = null)
        {
            BaseUri = new Uri(baseUri);

            Configuration = configuration ?? Configuration.Default();
            CookieContainer = new CookieContainer();
            Logging = new LoggingProvider(Configuration.Logger);

            CredentialCache = CredentialCacheFactory.GetCredentialCache();
            Serializer = GetSerializer();
            LoggingSerializer = GetLoggingSerializer(this);

            RegisterRequestProviders();

            // Add supported entities
            AccessControls = new AccessControlsEntity(this);
            AsyncOperations = new AsyncOperationsEntity(this);
            Capabilities = new CapabilitiesEntity(this);
            ConnectorGroups = new ConnectorGroupsEntity(this);
            Configs = new ConfigsEntity(this);
            FavoriteFolders = new FavoriteFoldersEntity(this);
            Groups = new GroupsEntity(this);
            Metadata = new MetadataEntity(this);
            Sessions = new SessionsEntity(this);
            Shares = new SharesEntity(this);
            Users = new UsersEntity(this);
#if ShareFile
            Accounts = new AccountsEntityInternal(this);
            Items = new ItemsEntityInternal(this);
            Devices = new DevicesEntityInternal(this); 
            StorageCenters = new StorageCentersEntityInternal(this);
            Zones = new ZonesEntityInternal(this);
#else
            Accounts = new AccountsEntity(this);
            Items = new ItemsEntity(this);
#endif
        }

#if ShareFile
        public IAccountsEntityInternal Accounts { get; private set; }
        public IDevicesEntityInternal Devices { get; private set; }
        public IItemsEntityInternal Items { get; private set; }
        public IStorageCentersEntityInternal StorageCenters { get; private set; }
        public IZonesEntityInternal Zones { get; private set; }
#else
        public IAccountsEntity Accounts { get; private set; }
        public IItemsEntity Items { get; private set; }
#endif
        public IAccessControlsEntity AccessControls { get; private set; }
        public IAsyncOperationsEntity AsyncOperations { get; private set; }
        public ICapabilitiesEntity Capabilities { get; private set; }
        public IConnectorGroupsEntity ConnectorGroups { get; private set; }
        public IConfigsEntity Configs { get; private set; }
        public IFavoriteFoldersEntity FavoriteFolders { get; private set; }
        public IGroupsEntity Groups { get; private set; }
        public IMetadataEntity Metadata { get; private set; }
        public ISessionsEntity Sessions { get; private set; }
        public ISharesEntity Shares { get; private set; }
        public IUsersEntity Users { get; private set; }

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
#if Async
            RegisterAsyncRequestProvider(new AsyncRequestProvider(this));
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

#if Async
        public AsyncThreadedFileUploader GetAsyncFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null)
        {
            uploadSpecificationRequest.Method = UploadMethod.Threaded;

            return new AsyncThreadedFileUploader(this, uploadSpecificationRequest, file, config);
        }

        public AsyncFileDownloader GetAsyncFileDownloader(Item itemToDownload, DownloaderConfig config = null)
        {
            return new AsyncFileDownloader(itemToDownload, this, config);
        }

#else


#if ShareFile
        public ThreadedFileUploader GetFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null, int? expirationDays = null)
        {
            uploadSpecificationRequest.Method = UploadMethod.Threaded;

            return new ThreadedFileUploader(this, uploadSpecificationRequest, file, config, expirationDays);
        }
#else        
        public ThreadedFileUploader GetFileUploader(UploadSpecificationRequest uploadSpecificationRequest, IPlatformFile file, FileUploaderConfig config = null)
        {
            uploadSpecificationRequest.Method = UploadMethod.Threaded;

            return new ThreadedFileUploader(this, uploadSpecificationRequest, file, config);
        }
#endif

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

#if ShareFile
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
#endif

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

        public virtual Task<Stream> ExecuteAsync(IStreamQuery stream, CancellationToken? token = null)
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(stream, token);
        }

        public virtual Stream Execute(IStreamQuery stream)
        {
            return RequestProviderFactory.GetSyncRequestProvider().Execute(stream);
        }

        public virtual Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken? token = null)
            where T : class
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(query, token);
        }

        public virtual T Execute<T>(IQuery<T> query)
            where T : class
        {
            return RequestProviderFactory.GetSyncRequestProvider().Execute(query);
        }

        public virtual Task ExecuteAsync(IQuery query, CancellationToken? token = null)
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(query, token);
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
