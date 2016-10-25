using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http.Headers;

namespace ShareFile.Api.Client.Exceptions
{
    public class WebAuthenticationException : Exception
    {
        public IEnumerable<string> WwwAuthenticateSchemes { get; set; }

        public ReadOnlyCollection<AuthenticationHeaderValue> WwwAuthenticateHeaders { get; private set; }

        public Uri RequestUri { get; set; }

        public WebAuthenticationException(
            string message,
            IEnumerable<AuthenticationHeaderValue> wwwAuthenticateHeaders = null,
            Exception innerException = null)
            : base(message, innerException)
        {
            WwwAuthenticateHeaders = new ReadOnlyCollection<AuthenticationHeaderValue>(wwwAuthenticateHeaders.ToList());
            WwwAuthenticateSchemes = WwwAuthenticateHeaders.Select(x => x.Scheme);
        }

        public WebAuthenticationException(string message, IEnumerable<string> wwwAuthenticateSchemes = null, Exception innerException = null) :
            base(message, innerException)
        {
            WwwAuthenticateSchemes = wwwAuthenticateSchemes;
        }

        public WebAuthenticationException(string message, string wwwAuthenticateSchemes = null, Exception innerException = null) :
            base(message, innerException)
        {
            if (!string.IsNullOrEmpty(wwwAuthenticateSchemes))
            {
                WwwAuthenticateSchemes = wwwAuthenticateSchemes.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }


    public class ProxyAuthenticationException : Exception
    {
        public ProxyAuthenticationException(string message, Exception innerException = null) :
            base(message, innerException)
        {

        }
    }

}
