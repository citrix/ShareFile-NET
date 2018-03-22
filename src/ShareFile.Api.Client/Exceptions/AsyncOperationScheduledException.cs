using System;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Exceptions
{
    public class AsyncOperationScheduledException : Exception
    {
        public AsyncOperationScheduledException(ODataFeed<AsyncOperation> asyncOperation)
        {
            ScheduledOperations = asyncOperation;
        }

        public ODataFeed<AsyncOperation> ScheduledOperations { get; set; }
    }
}
