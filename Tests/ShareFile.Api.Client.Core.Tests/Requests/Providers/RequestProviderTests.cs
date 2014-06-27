using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using FakeItEasy;
using Newtonsoft.Json;
using NUnit.Framework;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Requests.Executors;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Core.Tests.Requests.Providers
{
    public class RequestProviderTests : BaseTests
    {
        [TestCase(true, TestName = "Query_ItemNotFound_Async")]
        [TestCase(false, TestName = "Query_ItemNotFound_Sync")]
        public async void Query_ItemNotFound(bool async)
        {
            // Arrange
            var query = GetItemDeleteQuery();
            ConfigureNotFound();

            // Act
            try
            {
                if (async)
                    await query.ExecuteAsync();
                else query.Execute();
            }
            // Assert
            catch (ODataException exception)
            {
                Assert.IsTrue(exception.Code == HttpStatusCode.NotFound);
                return;
            }

            Assert.Fail();
        }

        [TestCase(true, TestName = "QueryT_ItemNotFound_Async")]
        [TestCase(false, TestName = "QueryT_ItemNotFound_Sync")]
        public async void QueryT_ItemNotFound(bool async)
        {
            var query = GetItemQuery();
            ConfigureNotFound();

            try
            {
                if (async)
                    await query.ExecuteAsync();
                else query.Execute();
            }
            catch (ODataException exception)
            {
                Assert.IsTrue(exception.Code == HttpStatusCode.NotFound);
                return;
            }

            Assert.Fail();
        }

        [TestCase(true, TestName = "QueryStream_ItemNotFound_Async")]
        [TestCase(false, TestName = "QueryStream_ItemNotFound_Sync")]
        public async void QueryStream_ItemNotFound(bool async)
        {
            var query = GetThumbnailQuery();
            ConfigureNotFound();

            try
            {
                if (async)
                    await query.ExecuteAsync();
                else query.Execute();
            }
            catch (ODataException exception)
            {
                Assert.IsTrue(exception.Code == HttpStatusCode.NotFound);
                return;
            }

            Assert.Fail();
        }

        protected IQuery GetItemDeleteQuery()
        {
            var client = GetShareFileClient(true);
            return client.Items.Delete(client.Items.GetAlias("fileId"));
        }

        protected IQuery<Item> GetItemQuery()
        {
            var client = GetShareFileClient(true);
            return client.Items.Get(client.Items.GetAlias("fileId"));
        }

        protected IQuery<Stream> GetThumbnailQuery()
        {
            var client = GetShareFileClient(true);
            return client.Items.GetThumbnail(client.Items.GetAlias("fileId"));
        }

        protected void ConfigureNotFound()
        {
            A.CallTo(() =>
                    RequestExecutorFactory.GetAsyncRequestExecutor()
                        .SendAsync(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored,
                            A<CancellationToken>.Ignored))
                .Returns(GenerateResponseMessage(HttpStatusCode.NotFound, "Items: NotFound"));

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                .Returns(GenerateResponseMessage(HttpStatusCode.NotFound, "Items: NotFound"));
        }

        protected HttpResponseMessage GenerateResponseMessage(HttpStatusCode statusCode, string message)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new ODataRequestException
                {
                    Code = statusCode,
                    Message = new ODataExceptionMessage { Language = "en-US", Message = message }
                }))
            };
        }
    }
}
