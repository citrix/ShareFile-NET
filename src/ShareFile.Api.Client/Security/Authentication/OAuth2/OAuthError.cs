using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using ShareFile.Api.Client.Extensions;

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
            string value;
            if (values.TryRemoveValue("error", out value))
            {
                Error = value != null ? Uri.UnescapeDataString(value.Replace('+', ' ')) : string.Empty;
            }
            else Error = string.Empty;
            if (values.TryRemoveValue("error_description", out value))
            {
                ErrorDescription = value != null ? Uri.UnescapeDataString(value.Replace('+', ' ')) : string.Empty;
            }
            else ErrorDescription = string.Empty;

            Properties = values;
        }

        public IDictionary<string, string> Properties { get; protected set; }

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

        public override string Message
        {
            get
            {
                if (Error == null)
                    return base.Message;
                return string.Format("Error: {0} | Description: {1}", Error.Error, Error.ErrorDescription);
            }
        }
    }
}
