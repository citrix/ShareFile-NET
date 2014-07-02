using System;

namespace ShareFile.Api.Client.Security.Cryptography
{
    public class HmacSha256ProviderFactory
    {
        private static Func<byte[], IHmacSha256Provider> _providerFunc;
        public static void Register(Func<byte[], IHmacSha256Provider> providerFunc)
        {
            _providerFunc = providerFunc;
        }

        public static IHmacSha256Provider GetProvider(byte[] key)
        {
            return _providerFunc(key);
        }

        public static bool HasProvider()
        {
            return _providerFunc != null;
        }
    }
}
