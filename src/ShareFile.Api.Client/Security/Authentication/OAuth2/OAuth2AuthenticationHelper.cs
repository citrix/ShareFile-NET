using System;
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Security.Authentication.OAuth2
{
    public class OAuth2AuthenticationHelper
    {
        private readonly string _completionUrl;

        public OAuth2AuthenticationHelper(Uri completionUri)
        {
            _completionUrl = completionUri.ToString();
        }

        public bool IsComplete(Uri navigationUri, out IOAuthResponse response)
        {
            response = null;
            if (navigationUri.ToString().StartsWith(_completionUrl))
            {
                var queryString = navigationUri.Query.ToQueryStringCollection();
                if (queryString == null) return false;

                response = queryString.ToOAuthResponse();
                return true;
            }

            return false;
        }
    }
}
