using System;
using System.Collections.Generic;

namespace ShareFile.Api.Client.Exceptions
{
    public class WebAuthenticationException : Exception
    {
        public IEnumerable<string> WwwAuthenticateSchemes { get; set; }
        public Uri RequestUri { get; set; }

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
