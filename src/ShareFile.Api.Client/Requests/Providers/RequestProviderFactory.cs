using System;

namespace ShareFile.Api.Client.Requests.Providers
{
    internal class RequestProviderFactory
    {
        private Func<ISyncRequestProvider> _syncRequestProviderFunc;
        private Func<IAsyncRequestProvider> _asyncRequestProviderFunc;

        internal void RegisterSyncRequestProvider(Func<ISyncRequestProvider> syncRequestProvider)
        {
            _syncRequestProviderFunc = syncRequestProvider;
        }

        public void RegisterAsyncRequestProvider(Func<IAsyncRequestProvider> asyncRequestProvider)
        {
            _asyncRequestProviderFunc = asyncRequestProvider;
        }

        public ISyncRequestProvider GetSyncRequestProvider()
        {
            return _syncRequestProviderFunc();
        }

        public IAsyncRequestProvider GetAsyncRequestProvider()
        {
            return _asyncRequestProviderFunc();
        }
    }
}
