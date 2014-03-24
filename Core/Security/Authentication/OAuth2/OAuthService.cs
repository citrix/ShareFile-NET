using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ShareFile.Api.Client.Requests;

namespace ShareFile.Api.Client.Security.Authentication.OAuth2
{
    public interface IOAuthService
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        Task<OAuthToken> ExchangeAuthorizationCodeAsync(OAuthAuthorizationCode code);
        Task<OAuthToken> RefreshOAuthTokenAsync(OAuthToken token);
        Task<OAuthToken> ExchangeSamlAssertionAsync(string samlAssertion, string subdomain, string applicationControlPlane);
        string GetAuthorizationUrl(string tld, string responseType, string clientId, string redirectUri, string state, Dictionary<string, string> additionalQueryStringParams = null, string subdomain = "secure");
    }

    public class OAuthService : IOAuthService
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public IShareFileClient ShareFileClient { get; set; }

        public OAuthService(IShareFileClient shareFileClient, string clientId, string clientSecret)
        {
            ClientId = clientId;
            ClientSecret = clientSecret;
            ShareFileClient = shareFileClient;
        }
        
        public Task<OAuthToken> ExchangeAuthorizationCodeAsync(OAuthAuthorizationCode code)
        {
            return RequestOAuthTokenAsync(code.ApplicationControlPlane,
                new Dictionary<string, string>
                {
                    {"client_id", ClientId},
                    {"client_secret", ClientSecret},
                    {"code", code.Code},
                    {"grant_type", "authorization_code"}
                }, code.Subdomain);
        }

        public Task<OAuthToken> RefreshOAuthTokenAsync(OAuthToken token)
        {
            return RequestOAuthTokenAsync(token.ApplicationControlPlane,
                new Dictionary<string, string>
                {
                    {"client_id", ClientId},
                    {"client_secret", ClientSecret},
                    {"refresh_token", token.RefreshToken},
                    {"grant_type", "refresh_token"}
                }, token.Subdomain);
        }

        public Task<OAuthToken> ExchangeSamlAssertionAsync(string samlAssertion, string subdomain, string applicationControlPlane)
        {
            return RequestOAuthTokenAsync(applicationControlPlane,
                new Dictionary<string, string>
                {
                    {"client_id", ClientId},
                    {"client_secret", ClientSecret},
                    {"grant_type", "urn:ietf:params:oauth:grant-type:saml2-bearer"},
                    {"assertion", samlAssertion}
                }, subdomain);
        }

        public Task<OAuthToken> PasswordGrantAsync(string username, string password, string subdomain,
            string applicationControlPlane)
        {
            return RequestOAuthTokenAsync(applicationControlPlane,
                new Dictionary<string, string>
                {
                    {"client_id", ClientId},
                    {"client_secret", ClientSecret},
                    {"grant_type", "password"},
                    {"username", username},
                    {"password", password}
                }, subdomain);
        }

        private async Task<OAuthToken> RequestOAuthTokenAsync(string applicationControlPlane, IEnumerable<KeyValuePair<string, string>> requestFormData, string subdomain = "secure")
        {
            for (int i = 0; i < 2; i++)
            {
                var url = string.Format("https://{0}.{1}/oauth/token", subdomain, applicationControlPlane);
                var oauthTokenQuery = new FormQuery<OAuthToken>(ShareFileClient)
                    .Ids(url)
                    .QueryString("requirev3", "true");

                oauthTokenQuery.Body = requestFormData;
                oauthTokenQuery.HttpMethod = "POST";

                try
                {
                    return await oauthTokenQuery.ExecuteAsync().ConfigureAwait(false);
                }
                catch (Exception)
                {
                    if (i >= 1)
                    {
                        throw;
                    }
                }
            }

            throw new Exception();
        }

        public string GetAuthorizationUrl(string tld, string responseType, string clientId, string redirectUri, string state, Dictionary<string, string> additionalQueryStringParams = null, string subdomain = "secure")
        {
            var sb = new StringBuilder();

            if(additionalQueryStringParams != null)
            {
                foreach (var kvp in additionalQueryStringParams)
                {
                    sb.AppendFormat("{0}={1}&", kvp.Key, kvp.Value);
                }
            }

            return
                string.Format(
                    "https://{0}.{1}/oauth/authorize?response_type={2}&client_id={3}&redirect_uri={4}&state={5}&{6}",
                    subdomain,
                    tld,
                    Uri.EscapeDataString(responseType),
                    Uri.EscapeDataString(clientId),
                    Uri.EscapeDataString(redirectUri),
                    Uri.EscapeDataString(state), 
                    sb.ToString().TrimEnd('&'));
        }
    }
}
