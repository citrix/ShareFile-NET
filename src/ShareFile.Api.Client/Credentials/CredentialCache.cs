using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace ShareFile.Api.Client.Credentials
{
    public class CredentialCache : NetworkCredential, ICredentialCache
    {
        private readonly Dictionary<string, List<CredentialAuthorityContainer>> _credentials = new Dictionary<string, List<CredentialAuthorityContainer>>();
        private readonly object _credentialLock = new object();

        public void Add(Uri uri, string authType, NetworkCredential credentials)
        {
            AddCredentialInternal(uri, authType, credentials);
        }

        public void Remove(Uri uri, string authType)
        {
            RemoveInternal(uri, authType);
        }

        public new NetworkCredential GetCredential(Uri uri, string authType)
        {
            return GetCredentialInternal(uri, authType).Credentials;
        }

        private string GetKey(Uri uri)
        {
            return uri.Host;
        }

        private CredentialAuthorityContainer GetCredentialInternal(Uri uri, string authType)
        {
            CredentialAuthorityContainer credential = null;
            List<CredentialAuthorityContainer> container;
            if (_credentials.TryGetValue(GetKey(uri), out container))
            {
                if (container.Count > 0)
                {
                    if (string.IsNullOrWhiteSpace(authType))
                    {
                        credential = container.FirstOrDefault();
                    }
                    else
                    {
                        credential =
                            container.FirstOrDefault(
                                x => x.AuthenticationType.Equals(authType, StringComparison.OrdinalIgnoreCase));
                    }
                }
            }

            return credential ?? CredentialAuthorityContainer.Default;
        }

        private void AddCredentialInternal(Uri uri, string authType, NetworkCredential credentials)
        {
            var existingCredentials = GetCredentialInternal(uri, authType);

            if (existingCredentials != CredentialAuthorityContainer.Default)
            {
                RemoveCredentialContainer(existingCredentials);
            }

            lock (_credentialLock)
            {
                List<CredentialAuthorityContainer> containerList;
                if (!_credentials.TryGetValue(GetKey(uri), out containerList))
                {
                    containerList = new List<CredentialAuthorityContainer>();
                }

                containerList.Add(new CredentialAuthorityContainer
                {
                    AuthenticationType = authType,
                    Uri = uri,
                    Credentials = credentials
                });

                _credentials[GetKey(uri)] = containerList;
            }
        }

        private void RemoveInternal(Uri uri, string authType)
        {
            var existingCredentials = GetCredentialInternal(uri, authType);
            if (existingCredentials != null && existingCredentials != CredentialAuthorityContainer.Default)
            {
                RemoveCredentialContainer(existingCredentials);
            }
        }

        private void RemoveCredentialContainer(CredentialAuthorityContainer container)
        {
            List<CredentialAuthorityContainer> containerList;
            if (_credentials.TryGetValue(GetKey(container.Uri), out containerList))
            {
                lock (_credentialLock)
                {
                    containerList.Remove(container);
                    _credentials[GetKey(container.Uri)] = containerList;
                }
            }
        }
    }

    internal class CredentialAuthorityContainer
    {
        internal Uri Uri { get; set; }
        internal string AuthenticationType { get; set; }
        internal NetworkCredential Credentials { get; set; }

        public CredentialAuthorityContainer()
        {
            // Latest System.Net.Http doesn't check if credentials are empty, but checks against this value
            Credentials = Configuration.IsNetCore
                              ? System.Net.CredentialCache.DefaultNetworkCredentials
                              : new NetworkCredential("", "");
        }

        internal static CredentialAuthorityContainer Default = new CredentialAuthorityContainer();
    }
}
