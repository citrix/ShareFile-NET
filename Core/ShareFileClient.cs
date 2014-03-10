using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShareFile.Api.Client.Converters;
using ShareFile.Api.Client.Credentials;
using ShareFile.Api.Client.Entities;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Requests.Providers;

namespace ShareFile.Api.Client
{
    public class ShareFileClient
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

            CredentialCache = CredentialCacheFactory.GetCredentialCache();
            Serializer = GetSerializer();

            RegisterRequestProviders();

            // Add supported entities
            AccessControls = new AccessControlsEntity(this);
            AsyncOperations = new AsyncOperationsEntity(this);
            Capabilities = new CapabilitiesEntity(this);
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
        public AccountsEntityInternal Accounts { get; private set; }
        public DevicesEntityInternal Devices { get; private set; }
        public ItemsEntityInternal Items { get; private set; }
        public StorageCentersEntityInternal StorageCenters { get; private set; }
        public ZonesEntityInternal Zones { get; private set; }
#else
        public AccountsEntity Accounts { get; private set; }
        public ItemsEntity Items { get; private set; }
#endif
        public AccessControlsEntity AccessControls { get; private set; }
        public AsyncOperationsEntity AsyncOperations { get; private set; }
        public CapabilitiesEntity Capabilities { get; private set; }
        public ConfigsEntity Configs { get; private set; }
        public FavoriteFoldersEntity FavoriteFolders { get; private set; }
        public GroupsEntity Groups { get; private set; }
        public MetadataEntity Metadata { get; private set; }
        public SessionsEntity Sessions { get; private set; }
        public SharesEntity Shares { get; private set; }
        public UsersEntity Users { get; private set; }

        public Uri NextRequestBaseUri { get; private set; }
        public Uri BaseUri { get; set; }

        public Configuration Configuration { get; set; }

        internal LoggingProvider Logging { get; set; }
        internal ICredentialCache CredentialCache { get; set; }
        internal CookieContainer CookieContainer { get; set; }
        internal readonly JsonSerializer Serializer;

        internal void RegisterRequestProviders()
        {
            var provider = new DefaultRequestProvider
            {
                Client = this
            };

            RequestProviderFactory.RegisterAsyncRequestProvider(() => provider);
            RequestProviderFactory.RegisterSyncRequestProvider(() => provider);
        }

        private static JsonSerializer GetSerializer()
        {
            return new JsonSerializer
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                Converters = { new ODataConverter(), new StringEnumConverter() }
            };
        }

        /// <summary>
        /// Get request base Uri for the next request executed by the client.  Will use <value>NextRequestBaseUri</value> if available.
        /// </summary>
        /// <returns></returns>
        public Uri GetRequestBaseUri()
        {
            var requestUri = BaseUri;

            if (NextRequestBaseUri != null)
            {
                requestUri = NextRequestBaseUri;
                NextRequestBaseUri = null;
            }

            return requestUri;
        }

        /// <summary>
        /// Set the next base Uri to be used for the next request executed by the client.  This value will only be used once.
        /// </summary>
        /// <param name="tempBaseUri"></param>
        public void SetBaseUriForNextRequest(Uri tempBaseUri)
        {
            NextRequestBaseUri = tempBaseUri;
        }

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

        public Task<Stream> ExecuteAsync(IStreamQuery stream, CancellationToken? token = null)
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(stream, token);
        }

        public Stream Execute(IStreamQuery stream)
        {
            return RequestProviderFactory.GetSyncRequestProvider().Execute(stream);
        }

        public Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken? token = null)
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(query, token);
        }

        public T Execute<T>(IQuery<T> query)
        {
            return RequestProviderFactory.GetSyncRequestProvider().Execute(query);
        }

        public Task ExecuteAsync(IQuery query, CancellationToken? token = null)
        {
            return RequestProviderFactory.GetAsyncRequestProvider().ExecuteAsync(query, token);
        }

        public void Execute(IQuery query)
        {
            RequestProviderFactory.GetSyncRequestProvider().Execute(query);
        }
    }
}
