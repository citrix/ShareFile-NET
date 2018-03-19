namespace ShareFile.Api.Client.Requests.Executors
{
    public class RequestExecutorFactory
    {
        private static ISyncRequestExecutor _syncRequestExecutor;

        public static void RegisterSyncRequestProvider(ISyncRequestExecutor syncRequestExecutor)
        {
            _syncRequestExecutor = syncRequestExecutor;
        }

        public static ISyncRequestExecutor GetSyncRequestExecutor()
        {
            return _syncRequestExecutor;
        }
		
        private static IAsyncRequestExecutor _asyncRequestExecutor;
        public static void RegisterAsyncRequestProvider(IAsyncRequestExecutor asyncRequestExecutor)
        {
            _asyncRequestExecutor = asyncRequestExecutor;
        }

        public static IAsyncRequestExecutor GetAsyncRequestExecutor()
        {
            return _asyncRequestExecutor;
        }
    }
}
