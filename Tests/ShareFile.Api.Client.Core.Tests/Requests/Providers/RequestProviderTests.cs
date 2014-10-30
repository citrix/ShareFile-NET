using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using FakeItEasy;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using ShareFile.Api.Client.Events;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Requests.Executors;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Core.Tests.Requests.Providers
{
    using ShareFile.Api.Client.Requests.Filters;

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

        [TestCase(true, TestName = "AsyncOperationScheduled_Async")]
        [TestCase(false, TestName = "AsyncOperationScheduled_Sync")]
        public async void AsyncOperationScheduled(bool async)
        {
            // Arrange
            var shareFileClient = GetShareFileClient(true);
            ConfigureAsyncOperationScheduled();

            var newItem = new Models.File
            {
                Parent = new Folder
                {
                    Id = "fo" + GetId(34)
                }
            };
            var query = shareFileClient.Items.Update(shareFileClient.Items.GetAlias("fi" + GetId(34)), newItem);

            // Act
            try
            {
                if (async)
                {
                    await query.ExecuteAsync();
                }
                else query.Execute();
            }
            // Assert
            catch (AsyncOperationScheduledException asyncOperationScheduledException)
            {
                Assert.Pass();
                return;
            }

            Assert.Fail();
        }

        [TestCase(true, TestName="ZoneUnavailableThrown_Async")]
        [TestCase(false, TestName="ZoneUnavailableThrown_Sync")]
        public async void ZoneUnvailableThrown(bool async)
        {
            var shareFileClient = GetShareFileClient(true);
            ConfigureZoneUnavailableResponse();

            var query = shareFileClient.Items.Get(shareFileClient.Items.GetAlias(GetId()));

            try
            {
                if (async)
                    await query.ExecuteAsync();
                else
                    query.Execute();
                Assert.Fail();
            }
            catch(ZoneUnavailableException)
            {
                Assert.Pass();
            }
        }

        [TestCase(true, TestName = "OnDomainChangeRaise_Async")]
        [TestCase(false, TestName = "OnDomainChangeRaise_Sync")]
        public async void OnDomainChangeRaised(bool async)
        {
            // Arrange
            var shareFileClient = GetShareFileClient(true);
            ConfigureDomainChangedResponse();

            var changeDomainRaised = false;
            shareFileClient.AddChangeDomainHandler((message, redirect) =>
            {
                changeDomainRaised = true;
                ConfigureItemResponse();
                return EventHandlerResponse.Redirect(redirect);
            });

            var query = shareFileClient.Items.Get(shareFileClient.Items.GetAlias(GetId()));

            // Act
            if (async)
            {
                await query.ExecuteAsync();
            }
            else
            {
                query.Execute();
            }

            // Assert
            changeDomainRaised.Should().BeTrue();
        }

        [TestCase(true, TestName = "WebAuthenticationException_Async")]
        [TestCase(false, TestName = "WebAuthenticationException_Sync")]
        public async void WebAuthenticationException(bool async)
        {
            // Arrange
            var query = GetItemQuery();
            ConfigureUnauthorizedResponse();

            try
            {
            // Act
                if (async)
                {
                    await query.ExecuteAsync();
                }
                else query.Execute();
            }
            // Assert
            catch (WebAuthenticationException exception)
            {
                Assert.Pass();
                return;
            }

            Assert.Fail();
        }

        [TestCase]
        public void EnsureImplicitAndFilter()
        {
            // Arrange
            var client = this.GetShareFileClient(true);
            var query = client.Items.GetChildren(client.Items.GetAlias("fileId"));
            
            // Act
            var endsWithFilter = new EndsWithFilter("Property", "value");
            var startsWithFilter = new StartsWithFilter("Property", "value");
            query = query.Filter(endsWithFilter).Filter(startsWithFilter);

            var odataQuery = query as Query<ODataFeed<Item>>;
            var filter = odataQuery.GetFilter();

            // Assert
            filter.Should().BeOfType<AndFilter>();
            (filter as AndFilter).Left.Should().Be(endsWithFilter);
            (filter as AndFilter).Right.Should().Be(startsWithFilter);
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
                .Returns(GenerateODataRequestException(HttpStatusCode.NotFound, "Items: NotFound"));

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                .Returns(GenerateODataRequestException(HttpStatusCode.NotFound, "Items: NotFound"));
        }

        protected void ConfigureAsyncOperationScheduled()
        {
            A.CallTo(() =>
                    RequestExecutorFactory.GetAsyncRequestExecutor()
                        .SendAsync(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored,
                            A<CancellationToken>.Ignored))
                .Returns(GenerateAsyncOperationScheduled());

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                .Returns(GenerateAsyncOperationScheduled());
        }

        protected void ConfigureDomainChangedResponse()
        {
            A.CallTo(() =>
                    RequestExecutorFactory.GetAsyncRequestExecutor()
                        .SendAsync(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored,
                            A<CancellationToken>.Ignored))
                .Returns(GenerateRedirectionResponse());

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                .Returns(GenerateRedirectionResponse());
        }

        protected void ConfigureZoneUnavailableResponse()
        {
            A.CallTo(() =>
                    RequestExecutorFactory.GetAsyncRequestExecutor()
                        .SendAsync(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored,
                            A<CancellationToken>.Ignored))
                .Returns(GenerateRedirectionUnavailableResponse());

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                .Returns(GenerateRedirectionUnavailableResponse());
        }

        protected void ConfigureItemResponse()
        {
            HttpRequestMessage requestMessage = null;

            A.CallTo(() =>
                    RequestExecutorFactory.GetAsyncRequestExecutor()
                        .SendAsync(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored,
                            A<CancellationToken>.Ignored))
                        .Invokes((HttpClient client, HttpRequestMessage message, HttpCompletionOption options, CancellationToken token) =>
                        {
                            requestMessage = message;
                        })
                .ReturnsLazily(() => GenerateItemResponse(requestMessage));

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                        .Invokes((HttpClient client, HttpRequestMessage message, HttpCompletionOption options) =>
                        {
                            requestMessage = message;
                        })
                .ReturnsLazily(() => GenerateItemResponse(requestMessage));
        }

        protected void ConfigureUnauthorizedResponse()
        {
            HttpRequestMessage requestMessage = null;
            A.CallTo(() =>
                    RequestExecutorFactory.GetAsyncRequestExecutor()
                        .SendAsync(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored,
                            A<CancellationToken>.Ignored))
                        .Invokes((HttpClient client, HttpRequestMessage message, HttpCompletionOption options, CancellationToken token) =>
                        {
                            requestMessage = message;
                        })
                .ReturnsLazily(() => GenerateUnauthorizedResponse(requestMessage));

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                        .Invokes((HttpClient client, HttpRequestMessage message, HttpCompletionOption options) =>
                        {
                            requestMessage = message;
                        })
                .ReturnsLazily(() => GenerateUnauthorizedResponse(requestMessage));
        }

        protected HttpResponseMessage GenerateODataRequestException(HttpStatusCode statusCode, string message)
        {
            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new ODataRequestException
                {
                    Code = statusCode,
                    Message = new ODataExceptionMessage { Language = "en-US", Message = message }
                }), Encoding.UTF8, "application/json")
            };
        }

        protected HttpResponseMessage GenerateAsyncOperationScheduled()
        {
            return new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new AsyncOperation
                {
                    BatchID = GetId(10),
                    BatchSourceID = GetId(),
                    BatchProgress = 0,
                    BatchState = SafeEnum<AsyncOperationState>.Create(AsyncOperationState.Scheduled)
                }), Encoding.UTF8, "application/json")
            };
        }

        protected HttpResponseMessage GenerateRedirectionResponse()
        {
            var baseUri = "https://newhost.sharefile.com/sf/v3/";
            var redirection = new Redirection
            {
                Available = true,
                Uri = new Uri(baseUri)
            };

            redirection.MetadataUrl = "https://newhost.sharefile.com/sf/v3/$metadata#ShareFile.Api.Models.Redirection@Element";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(redirection), Encoding.UTF8, "application/json"),
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("https://secure.sf-api.com/sf/v3/Items(" + GetId() + ")"))
            };
        }

        protected HttpResponseMessage GenerateRedirectionUnavailableResponse()
        {
            var redirection = new Redirection
            {
                Available = false
            };

            redirection.MetadataUrl = "https://newhost.sharefile.com/sf/v3/$metadata#ShareFile.Api.Models.Redirection@Element";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(redirection), Encoding.UTF8, "application/json"),
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("https://secure.sf-api.com/sf/v3/Items(" + GetId() + ")"))
            };

        }

        protected HttpResponseMessage GenerateItemResponse(HttpRequestMessage requestMessage)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new 
                {
                    Id = GetId(),
                    Name = "Test Item",
                    odatatype = "ShareFile.Api.Models.Item"
                }).Replace("odatatype", "odata.type"), Encoding.UTF8, "application/json"),
                RequestMessage = requestMessage
            };
        }

        protected HttpResponseMessage GenerateUnauthorizedResponse(HttpRequestMessage requestMessage)
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new ODataRequestException
                {
                    Code = HttpStatusCode.Unauthorized,
                    Message = new ODataExceptionMessage { Language = "en-US", Message = "Unauthorized" }
                }), Encoding.UTF8, "application/json"),
                RequestMessage = requestMessage
            };

            responseMessage.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer"));

            return responseMessage;
        }
    }
}
