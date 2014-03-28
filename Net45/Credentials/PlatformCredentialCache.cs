namespace ShareFile.Api.Client.Credentials
{
    public class PlatformCredentialCache : System.Net.CredentialCache, ICredentialCache
    {
        static PlatformCredentialCache()
        {
            CredentialCacheFactory.RegisterCredentialCache(() => new PlatformCredentialCache());
        }
    }
}
