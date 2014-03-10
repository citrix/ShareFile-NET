using System;

namespace ShareFile.Api.Client
{
    public static class CredentialCacheFactory
    {
        private static Func<ICredentialCache> _credentialCacheFunc;

        public static void RegisterCredentialCache(Func<ICredentialCache> credentialCacheFunc)
        {
            
        }

        public static ICredentialCache GetCredentialCache()
        {
            if (_credentialCacheFunc != null)
            {
                return _credentialCacheFunc();
            }
            return new CredentialCache();
        }
    }
}