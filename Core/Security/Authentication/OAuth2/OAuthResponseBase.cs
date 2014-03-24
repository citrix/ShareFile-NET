using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShareFile.Api.Client.Security.Authentication.OAuth2
{
    public class OAuthResponseBase : IOAuthResponse
    {
        [JsonProperty("appcp")]
        public string ApplicationControlPlane { get; set; }

        [JsonProperty("apicp")]
        public string ApiControlPlane { get; set; }

        public string Subdomain { get; set; }

        private long _expiresIn;

        [JsonProperty("expires_in")]
        public long ExpiresIn
        {
            get
            {
                return _expiresIn;
            }
            set
            {
                _expiresIn = value;
                ExpirationDate = DateTimeOffset.UtcNow.AddSeconds(value);
            }
        }

        public DateTimeOffset ExpirationDate { get; set; }

        public virtual void Fill(IDictionary<string, string> values)
        {
            ExpiresIn = values.ContainsKey("expires_in") ? Convert.ToInt64(values["expires_in"]) : 0;
            ApplicationControlPlane = values.ContainsKey("appcp") ? values["appcp"] : "";
            ApiControlPlane = values.ContainsKey("apicp") ? values["apicp"] : "";
            Subdomain = values.ContainsKey("subdomain") ? values["subdomain"] : "";
        }
    }
}
