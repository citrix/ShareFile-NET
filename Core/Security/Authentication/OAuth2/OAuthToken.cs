using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ShareFile.Api.Client.Extensions;

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
            string value;
            if (values.TryRemoveValue("access_token", out value))
            {
                AccessToken = value;
            }
            if (values.TryRemoveValue("refresh_token", out value))
            {
                RefreshToken = value;
            }
            if (values.TryRemoveValue("token_type", out value))
            {
                TokenType = value;
            }
            base.Fill(values);
        }
        
        public Uri GetUri()
        {
            return new Uri(string.Format("https://{0}.{1}/sf/v3/", Subdomain, ApiControlPlane));
        }
    }
}