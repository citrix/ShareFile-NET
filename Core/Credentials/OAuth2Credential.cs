using System.Net;
using ShareFile.Api.Client.Security.Authentication.OAuth2;

namespace ShareFile.Api.Client.Credentials
{
    public class OAuth2Credential : NetworkCredential
    {
        public OAuthToken OAuthToken { get; set; }

        public OAuth2Credential(string oauthToken)
            : base("", oauthToken)
        {

        }

        public OAuth2Credential(OAuthToken token)
            : base ("", token.AccessToken)
        {
            OAuthToken = token;
        }
    }
}
