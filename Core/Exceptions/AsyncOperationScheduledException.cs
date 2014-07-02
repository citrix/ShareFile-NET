using System;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Exceptions
{
    public class AsyncOperationScheduledException : Exception
    {
        public AsyncOperationScheduledException(AsyncOperation asyncOperation)
        {
            ScheduledOperation = asyncOperation;
        }

        public AsyncOperation ScheduledOperation { get; set; }
    }
}
