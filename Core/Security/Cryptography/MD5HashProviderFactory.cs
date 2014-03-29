using System;

namespace ShareFile.Api.Client.Security.Cryptography
{
// ReSharper disable InconsistentNaming
    public static class MD5HashProviderFactory
// ReSharper restore InconsistentNaming
    {
        private static Func<IMD5HashProvider> _hashProviderFunc;

        public static void RegisterHashProvider(Func<IMD5HashProvider> hashProviderFunc)
        {
            _hashProviderFunc = hashProviderFunc;
        }

        public static IMD5HashProvider GetHashProvider()
        {
            if (_hashProviderFunc == null)
            {
                return new PortableMD5HashProvider();
            }

            return _hashProviderFunc();
        }
    }
}
