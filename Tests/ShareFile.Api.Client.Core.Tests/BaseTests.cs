using System;
using System.Text;

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

        private Random Random = new Random();
        protected string RandomString(int length)
        {
            var builder = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * Random.NextDouble() + 65)));
                builder.Append(ch);
            }
            return builder.ToString();
        }

        protected string GetId(int length = 36)
        {
            if (length > 36) length = 36;
            return Guid.NewGuid().ToString().Substring(0, length);
        }
    }
}
