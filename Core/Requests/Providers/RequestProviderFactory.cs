using System;

namespace ShareFile.Api.Client.Requests.Providers
{
    public class RequestProviderFactory
    {
        private static Func<ISyncRequestProvider> _syncRequestProviderFunc;
        private static Func<IAsyncRequestProvider> _asyncRequestProviderFunc;

        public static void RegisterSyncRequestProvider(Func<ISyncRequestProvider> syncRequestProviderFunc)
        {
            _syncRequestProviderFunc = syncRequestProviderFunc;
        }

        public static void RegisterAsyncRequestProvider(Func<IAsyncRequestProvider> asyncRequestProviderFunc)
        {
            _asyncRequestProviderFunc = asyncRequestProviderFunc;
        }

        public static ISyncRequestProvider GetSyncRequestProvider()
        {
            return _syncRequestProviderFunc();
        }

        public static IAsyncRequestProvider GetAsyncRequestProvider()
        {
            return _asyncRequestProviderFunc();
        }
    }
}
