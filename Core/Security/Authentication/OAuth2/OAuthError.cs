using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ShareFile.Api.Client.Security.Authentication.OAuth2
{
    public class OAuthError : IOAuthResponse
    {
        [JsonProperty("error")]
        public string Error { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }

        public void Fill(IDictionary<string, string> values)
        {
            Error = values.ContainsKey("error") ? values["error"] : "";
            ErrorDescription = values.ContainsKey("error_description") ? values["error_description"] : "";
        }

        public OAuthError(IDictionary<string, string> values)
        {
            Fill(values);
        }

        public OAuthError()
        {

        }
    }

    public class OAuthErrorException : Exception
    {
        public OAuthError Error { get; set; }
    }
}
