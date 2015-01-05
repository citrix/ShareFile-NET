using System.Collections.Generic;
using ShareFile.Api.Client.Security.Authentication.OAuth2;

namespace ShareFile.Api.Client.Extensions
{
    internal static class DictionaryExtensions
    {
        internal static IOAuthResponse ToOAuthResponse(this Dictionary<string, string> value)
        {
            IOAuthResponse response;
            
            if (value.ContainsKey("code"))
            {
                response = new OAuthAuthorizationCode();
            }
            else if (value.ContainsKey("access_token"))
            {
                response = new OAuthToken();
            }
            else if (value.ContainsKey("error"))
            {
                response = new OAuthError();
            }
            else response = new OAuthResponseBase();
            
            response.Fill(value);
            return response;
        }

        internal static bool TryRemoveValue(this IDictionary<string, string> values, string key, out string value)
        {
            if (values != null)
            {
                if (values.TryGetValue(key, out value))
                {
                    values.Remove(key);
                    return true;
                }
            }
            value = string.Empty;
            return false;
        }
    }
}
