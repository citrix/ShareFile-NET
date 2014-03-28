using System;
using System.Collections.Generic;
using System.Net;

namespace ShareFile.Api.Client.Credentials
{
    public class CredentialCache : ICredentialCache
    {
        private readonly Dictionary<string, NetworkCredential> _credentials = new Dictionary<string, NetworkCredential>();
        private readonly object _credentialLock = new object();

        public void Add(Uri uri, string authType, NetworkCredential credentials)
        {
            lock (_credentialLock)
            {
                _credentials.Add(uri.Host + authType, credentials);
            }
        }

        public void Remove(Uri uri, string authType)
        {
            lock (_credentialLock)
            {
                _credentials.Remove(GetKey(uri, authType));
            }
        }

        public NetworkCredential GetCredential(Uri uri, string authType)
        {
            lock (_credentialLock)
            {
                NetworkCredential credential = null;
                if (_credentials.TryGetValue(GetKey(uri, authType), out credential))
                {
                    return credential;
                }
            }
            return new NetworkCredential();
        }

        private string GetKey(Uri uri, string authType)
        {
            return uri.Host + authType;
        }
    }
}
