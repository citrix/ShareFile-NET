using System;
using System.Collections.Generic;
using System.Net;

namespace ShareFile.Api.Client
{
    public class CredentialCache : ICredentialCache
    {
        private readonly Dictionary<string, ICredentials> _credentials = new Dictionary<string, ICredentials>();
        private readonly object _credentialLock = new object();

        public void Add(Uri uri, string authType, ICredentials credentials)
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
                ICredentials credential = null;
                if (_credentials.TryGetValue(GetKey(uri, authType), out credential))
                {
                    return credential as NetworkCredential;
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
