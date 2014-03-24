using System;

namespace ShareFile.Api.Client.Security.Authentication
{
    public static class WebAuthenticationFactory
    {
        private static Func<IWebAuthentication<WebAuthenticationResults>> _instanceFunc;
        public static void Register(Func<IWebAuthentication<WebAuthenticationResults>> instanceFunc)
        {
            _instanceFunc = instanceFunc;
        }

        public static IWebAuthentication<WebAuthenticationResults> Create()
        {
            return _instanceFunc();
        }
    }
}
