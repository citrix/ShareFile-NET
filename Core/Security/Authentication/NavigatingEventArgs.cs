using System;
using System.Collections.Generic;

namespace ShareFile.Api.Client.Security.Authentication
{
    public class NavigatingEventArgs : EventArgs
    {
        public Uri Uri { get; set; }
        public bool IsDomainChange { get; set; }
        public bool CancelNavigation { get; set; }
    }

    public class WebAuthenticationFailedException : Exception
    {
        
    }

    public class WebAuthenticationResults
    {
        public Dictionary<string, string> Results { get; set; }
    }
}
