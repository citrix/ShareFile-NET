#if NET45 || NET462
namespace ShareFile.Api.Client.Credentials
{
    public class PlatformCredentialCache : System.Net.CredentialCache, ICredentialCache
    {
        public static void Register()
        {
            CredentialCacheFactory.RegisterCredentialCache(() => new PlatformCredentialCache());
        }
    }
}
#endif