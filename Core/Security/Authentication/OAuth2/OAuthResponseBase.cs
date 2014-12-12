using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ShareFile.Api.Client.Extensions;

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
            string value;
            if (values.TryRemoveValue("expires_in", out value))
            {
                long expiresIn;
                if (!Int64.TryParse(value, out expiresIn))
                {
                    expiresIn = 0;
                }
                ExpiresIn = expiresIn;
            }
            if (values.TryRemoveValue("appcp", out value))
            {
                ApplicationControlPlane = value;
            }
            if (values.TryRemoveValue("apicp", out value))
            {
                ApiControlPlane = value;
            }
            if (values.TryRemoveValue("subdomain", out value))
            {
                Subdomain = value;
            }

            Properties = values;
        }

        public IDictionary<string, string> Properties { get; set; }
    }
}
