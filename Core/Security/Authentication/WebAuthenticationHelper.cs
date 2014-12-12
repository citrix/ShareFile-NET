using System;
using System.Collections.Generic;
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Security.Authentication
{
    public class WebAuthenticationHelper
    {
        private readonly string _completionUrl;

        public WebAuthenticationHelper(Uri completionUri)
        {
            _completionUrl = completionUri.ToString();
        }

        public bool IsComplete(Uri navigationUri, out Dictionary<string, string> results)
        {
            results = null;
            if (navigationUri.ToString().StartsWith(_completionUrl))
            {
                results = navigationUri.Query.ToQueryStringCollection();
                return true;
            }
            return false;
        }
    }
}
