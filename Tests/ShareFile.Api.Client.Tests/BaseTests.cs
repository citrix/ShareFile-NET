using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using NUnit.Framework;
using FakeItEasy;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ShareFile.Api.Client.Converters;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Requests.Executors;

namespace ShareFile.Api.Client.Core.Tests
{
    public abstract class BaseTests
    {
        protected const string BaseUriString = "https://release.sf-api.com/sf/v3/";
        protected IShareFileClient GetShareFileClient(bool registerFakeExecutors = false)
        {
            var config = Configuration.Default();
            var logger = new DefaultLoggingProvider
            {
                LogLevel = LogLevel.Debug | LogLevel.Error | LogLevel.Fatal | LogLevel.Info | LogLevel.Trace |
                           LogLevel.Warn
            };

            config.Logger = logger;

            var client = new ShareFileClient(BaseUriString, Configuration.Default());

            if (registerFakeExecutors)
            {
                RequestExecutorFactory.RegisterAsyncRequestProvider(A.Fake<IAsyncRequestExecutor>());
                RequestExecutorFactory.RegisterSyncRequestProvider(A.Fake<ISyncRequestExecutor>());
            }

            return client;
        }

        protected UserInfo GetUserInfo()
        {
            try
            {
                using (var fileStream = System.IO.File.OpenRead(Path.Combine(TestContext.CurrentContext.TestDirectory, "TestConfig.json")))
                using (var streamReader = new StreamReader(fileStream))
                {
                    var info = streamReader.ReadToEnd();
                    return JsonConvert.DeserializeObject<UserInfo>(info);
                }
            }
            catch (Exception exception)
            {
                Assert.Fail(string.Format("No UserInfo found in TestConfig.json. Exception: {0}", exception));
                throw;
            }
        }

        protected JsonSerializer GetSerializer()
        {
            return new JsonSerializer
            {
                ObjectCreationHandling = ObjectCreationHandling.Replace,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore,
                DateTimeZoneHandling = DateTimeZoneHandling.Local,
                Converters = { new ODataConverter(), new StringEnumConverter(), new SafeEnumConverter() }
            };
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

        protected HttpResponseMessage GenerateODataObjectResponse(HttpRequestMessage requestMessage, string response)
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(response, Encoding.UTF8, "application/json"),
                RequestMessage = requestMessage
            };

            return responseMessage;
        }
    }
}
