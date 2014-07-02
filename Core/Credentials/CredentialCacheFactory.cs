using System;

namespace ShareFile.Api.Client.Credentials
{
    public static class CredentialCacheFactory
    {
        private static Func<ICredentialCache> _credentialCacheFunc;

        public static void RegisterCredentialCache(Func<ICredentialCache> credentialCacheFunc)
        {
            _credentialCacheFunc = credentialCacheFunc;
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