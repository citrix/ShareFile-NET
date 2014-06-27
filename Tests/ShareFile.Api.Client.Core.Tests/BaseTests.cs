using System;
using FakeItEasy;
using ShareFile.Api.Client.Requests.Executors;

namespace ShareFile.Api.Client.Core.Tests
{
    public abstract class BaseTests
    {
        protected const string BaseUriString = "https://release.sf-api.com/sf/v3/";
        protected IShareFileClient GetShareFileClient(bool registerFakeExecutors = false)
        {
            var client = new ShareFileClient(BaseUriString);

            if (registerFakeExecutors)
            {
                RequestExecutorFactory.RegisterAsyncRequestProvider(A.Fake<IAsyncRequestExecutor>());
                RequestExecutorFactory.RegisterSyncRequestProvider(A.Fake<ISyncRequestExecutor>());
            }

            return client;
        }

        protected string GetId(int length = 36)
        {
            if (length > 36) length = 36;
            return Guid.NewGuid().ToString().Substring(0, length);
        }
    }
}
