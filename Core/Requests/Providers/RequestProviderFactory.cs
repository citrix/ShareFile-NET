using System;

namespace ShareFile.Api.Client.Requests.Providers
{
    internal class RequestProviderFactory
    {
        private ISyncRequestProvider _syncRequestProvider;
        private IAsyncRequestProvider _asyncRequestProvider;

        internal void RegisterSyncRequestProvider(ISyncRequestProvider syncRequestProvider)
        {
            _syncRequestProvider = syncRequestProvider;
        }

        public void RegisterAsyncRequestProvider(IAsyncRequestProvider asyncRequestProvider)
        {
            _asyncRequestProvider = asyncRequestProvider;
        }

        public ISyncRequestProvider GetSyncRequestProvider()
        {
            return _syncRequestProvider;
        }

        public IAsyncRequestProvider GetAsyncRequestProvider()
        {
            return _asyncRequestProvider;
        }
    }
}
