using System;

namespace ShareFile.Api.Client.Requests.Providers
{
    public class RequestProviderFactory
    {
        private static ISyncRequestProvider _syncRequestProvider;
        private static IAsyncRequestProvider _asyncRequestProvider;

        public static void RegisterSyncRequestProvider(Func<ISyncRequestProvider> syncRequestProviderFunc)
        {
            _syncRequestProvider = syncRequestProviderFunc();
        }

        public static void RegisterAsyncRequestProvider(Func<IAsyncRequestProvider> asyncRequestProviderFunc)
        {
            _asyncRequestProvider = asyncRequestProviderFunc();
        }

        public static ISyncRequestProvider GetSyncRequestProvider()
        {
            return _syncRequestProvider;
        }

        public static IAsyncRequestProvider GetAsyncRequestProvider()
        {
            return _asyncRequestProvider;
        }
    }
}
