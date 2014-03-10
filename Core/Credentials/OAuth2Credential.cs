using System.Net;

namespace ShareFile.Api.Client.Credentials
{
    public class OAuth2Credential : NetworkCredential
    {
        public OAuth2Credential(string oauthToken)
            : base("", oauthToken)
        {

        }
    }
}
