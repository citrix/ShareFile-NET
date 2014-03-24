using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShareFile.Api.Client.Security.Authentication.OAuth2
{
    public class OAuthToken : OAuthResponseBase
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        public override void Fill(IDictionary<string, string> values)
        {
            base.Fill(values);
            string value;
            if (values.TryGetValue("access_token", out value))
            {
                AccessToken = value;
            }
            if (values.TryGetValue("refresh_token", out value))
            {
                RefreshToken = value;
            }
            if (values.TryGetValue("token_type", out value))
            {
                TokenType = value;
            }
        }
        
        public Uri GetUri()
        {
            return new Uri(string.Format("https://{0}.{1}/sf/v3/", Subdomain, ApiControlPlane));
        }
    }
}