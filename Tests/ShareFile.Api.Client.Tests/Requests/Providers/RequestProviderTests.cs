using System;
using System.IO;
using System.Linq.Expressions;
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
using ShareFile.Api.Client.Requests.Filters;
using ShareFile.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace ShareFile.Api.Client.Core.Tests.Requests.Providers
{
    public class RequestProviderTests : BaseTests
    {
        [TestCase(true, TestName = "GetStream_Async")]
        [TestCase(false, TestName = "GetStream_Sync")]
        public async void GetStream(bool async)
        {
            // Arrange
            var query = this.GetThumbnailQuery();
            var streamMessage = Guid.NewGuid().ToString();
            ConfigureStreamResponse(streamMessage);

            Stream stream;
            // Act
            if (async)
            {
                stream = await query.ExecuteAsync();
            }
            else
            {
                stream = query.Execute();
            }

            // Assert
            Assert.IsNotNull(stream);
        }

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

        [TestCase(true, null)]
        [TestCase(false, null)]
        [TestCase(true, "123")]
        [TestCase(false, "123")]
        public async void RedirectionWithRoot(bool async, string root)
        {
            // Arrange
            var shareFileClient = GetShareFileClient(true);
            ConfigureDomainChangedResponse(root);

            var changeDomainRaised = false;
            shareFileClient.AddChangeDomainHandler((message, redirect) =>
            {
                changeDomainRaised = true;
                ConfigureItemResponse(root);
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

        [TestCase(true)]
        [TestCase(false)]
        public async void DeleteRedirectionSupport(bool async)
        {
            // Arrange
            var shareFileClient = GetShareFileClient(true);
            ConfigureDomainChangedResponse();

            var changeDomainRaised = false;
            shareFileClient.AddChangeDomainHandler((message, redirect) =>
            {
                changeDomainRaised = true;
                ConfigureNoContentResponse();
                return EventHandlerResponse.Redirect(redirect);
            });

            var query = shareFileClient.Items.Delete(shareFileClient.Items.GetAlias(GetId()));

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

        [TestCase(HttpStatusCode.Moved)]
        [TestCase(HttpStatusCode.Redirect)]
        public void HttpsToHttpRedirect(HttpStatusCode code)
        {
            // Arrange
            var client = GetShareFileClient(true);
            var response = new HttpResponseMessage();
            response.StatusCode = code;
            response.Headers.Location = new Uri("http://example.com");
            A.CallTo(
                () =>
                RequestExecutorFactory.GetAsyncRequestExecutor()
                    .SendAsync(
                        A<HttpClient>.Ignored,
                        A<HttpRequestMessage>.Ignored,
                        A<HttpCompletionOption>.Ignored,
                        A<CancellationToken>.Ignored)).Returns(response);
            var query = client.Items.Get();

            // Act
            Action a = () => query.ExecuteAsync().Wait();
            a.ShouldThrow<AggregateException>().WithInnerException<HttpsExpectedException>();
        }

        [TestCase(HttpStatusCode.Moved)]
        [TestCase(HttpStatusCode.Redirect)]
        public void FollowRelativeUrlRedirect(HttpStatusCode code)
        {
            // Arrange
            var client = GetShareFileClient(registerFakeExecutors: true);
            Uri requestedItemUri = client.Items.GetAlias(Enums.ItemAlias.Home);
            Uri relativeItemUri = new Uri("/item", UriKind.Relative);
            Uri absoluteItemUri = new Uri(requestedItemUri, relativeItemUri);
            var response = new HttpResponseMessage();
            response.StatusCode = code;
            response.Headers.Location = relativeItemUri;
            response.RequestMessage = new HttpRequestMessage(HttpMethod.Get, requestedItemUri);
            A.CallTo(
                () =>
                RequestExecutorFactory.GetAsyncRequestExecutor()
                    .SendAsync(
                        A<HttpClient>.Ignored,
                        A<HttpRequestMessage>.That.Matches(i => i.RequestUri != absoluteItemUri),
                        A<HttpCompletionOption>.Ignored,
                        A<CancellationToken>.Ignored)).Returns(response);
            A.CallTo(
                () =>
                RequestExecutorFactory.GetAsyncRequestExecutor()
                    .SendAsync(
                        A<HttpClient>.Ignored,
                        A<HttpRequestMessage>.That.Matches(
                            i =>
                            i.Method == HttpMethod.Get
                            && i.RequestUri == absoluteItemUri),
                        A<HttpCompletionOption>.Ignored,
                        A<CancellationToken>.Ignored))
                .Returns(GenerateODataObjectResponse(new HttpRequestMessage(),
                @"{ ""Name"":""file.jpg"",
                    ""odata.metadata"":""https://citrix.sf-api.com/sf/v3/$metadata#Items/ShareFile.Api.Models.File@Element"",
                    ""odata.type"":""ShareFile.Api.Models.File"",}"));

            // Act
            var item = client.Items.Get(requestedItemUri).ExecuteAsync().Result;

            // Assert
            item.Name.Should().Be("file.jpg");
        }

        [TestCase(HttpStatusCode.Moved)]
        [TestCase(HttpStatusCode.Redirect)]
        public void FollowRedirect(HttpStatusCode code)
        {
            // Arrange
            var client = GetShareFileClient(true);
            var response = new HttpResponseMessage();
            response.StatusCode = code;
            response.Headers.Location = new Uri("https://example.com");
            A.CallTo(
                () =>
                RequestExecutorFactory.GetAsyncRequestExecutor()
                    .SendAsync(
                        A<HttpClient>.Ignored,
                        A<HttpRequestMessage>.That.Matches(i => i.RequestUri != new Uri("https://example.com")),
                        A<HttpCompletionOption>.Ignored,
                        A<CancellationToken>.Ignored)).Returns(response);
            A.CallTo(
                () =>
                RequestExecutorFactory.GetAsyncRequestExecutor()
                    .SendAsync(
                        A<HttpClient>.Ignored,
                        A<HttpRequestMessage>.That.Matches(
                            i =>
                            i.Method == HttpMethod.Get
                            && i.RequestUri == new Uri("https://example.com")),
                        A<HttpCompletionOption>.Ignored,
                        A<CancellationToken>.Ignored))
                .Returns(GenerateODataObjectResponse(new HttpRequestMessage(),
                @"{ ""Name"":""file.jpg"",
                    ""odata.metadata"":""https://citrix.sf-api.com/sf/v3/$metadata#Items/ShareFile.Api.Models.File@Element"",
                    ""odata.type"":""ShareFile.Api.Models.File"",}"));

            // Act
            var item = client.Items.Get().ExecuteAsync().Result;

            // Assert
            item.Name.Should().Be("file.jpg");
        }

        [TestCase(HttpStatusCode.Moved)]
        [TestCase(HttpStatusCode.Redirect)]
        public void ExceedMaximumRedirectionCount(HttpStatusCode code)
        {
            // Arrange
            var client = GetShareFileClient(true);
            var response = new HttpResponseMessage();
            response.StatusCode = code;
            response.Headers.Location = new Uri("https://example.com");
            A.CallTo(
                () =>
                RequestExecutorFactory.GetAsyncRequestExecutor()
                    .SendAsync(
                        A<HttpClient>.Ignored,
                        A<HttpRequestMessage>.Ignored,
                        A<HttpCompletionOption>.Ignored,
                        A<CancellationToken>.Ignored)).Returns(response);

            // Act
            Action a = () => client.Items.Get().ExecuteAsync().Wait();

            // Assert
            a.ShouldThrow<AggregateException>().WithInnerException<HttpRequestException>().WithInnerMessage("Exceeded maximum number of allowed redirects.");
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

        protected void ConfigureStreamResponse(string responseContent)
        {
            A.CallTo(() =>
                    RequestExecutorFactory.GetAsyncRequestExecutor()
                        .SendAsync(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored,
                            A<CancellationToken>.Ignored))
                .Returns(this.GenerateStreamResponse(responseContent));

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                .Returns(this.GenerateStreamResponse(responseContent));
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

        protected void ConfigureDomainChangedResponse(string root = null)
        {
            A.CallTo(() =>
                    RequestExecutorFactory.GetAsyncRequestExecutor()
                        .SendAsync(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored,
                            A<CancellationToken>.Ignored))
                .Returns(GenerateRedirectionResponse(root));

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                .Returns(GenerateRedirectionResponse(root));
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

        protected void ConfigureItemResponse(string root = null)
        {
            HttpRequestMessage requestMessage = null;

            A.CallTo(() =>
                    RequestExecutorFactory.GetAsyncRequestExecutor()
                        .SendAsync(A<HttpClient>.Ignored, A<HttpRequestMessage>.That.Matches(
                            message => root == null || message.RequestUri.ToString().Contains("root=" + root)), A<HttpCompletionOption>.Ignored,
                            A<CancellationToken>.Ignored))
                        .Invokes((HttpClient client, HttpRequestMessage message, HttpCompletionOption options, CancellationToken token) =>
                        {
                            requestMessage = message;
                        })
                .ReturnsLazily(() => GenerateItemResponse(requestMessage));

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.That.Matches(
                            message => root == null || message.RequestUri.ToString().Contains("root=" + root)), A<HttpCompletionOption>.Ignored))
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

        protected void ConfigureODataObjectResponse(string response)
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
                .ReturnsLazily(() => GenerateODataObjectResponse(requestMessage, response));

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                        .Invokes((HttpClient client, HttpRequestMessage message, HttpCompletionOption options) =>
                        {
                            requestMessage = message;
                        })
                .ReturnsLazily(() => GenerateODataObjectResponse(requestMessage, response));
        }

        protected void ConfigureNoContentResponse()
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
                .ReturnsLazily(() => GenerateNoContentResponse(requestMessage));

            A.CallTo(() =>
                    RequestExecutorFactory.GetSyncRequestExecutor()
                        .Send(A<HttpClient>.Ignored, A<HttpRequestMessage>.Ignored, A<HttpCompletionOption>.Ignored))
                        .Invokes((HttpClient client, HttpRequestMessage message, HttpCompletionOption options) =>
                        {
                            requestMessage = message;
                        })
                .ReturnsLazily(() => GenerateNoContentResponse(requestMessage));
        }

        protected HttpResponseMessage GenerateODataRequestException(HttpStatusCode statusCode, string message)
        {
            return new HttpResponseMessage(statusCode)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri("https://secure.sf-api.com/sf/v3/")),
                Content = new StringContent(SerializeObject(new ODataRequestException
                {
                    Code = statusCode,
                    Message = new ODataExceptionMessage { Language = "en-US", Message = message }
                }), Encoding.UTF8, "application/json")
            };
        }

        private HttpResponseMessage streamResponse;
        protected HttpResponseMessage GenerateStreamResponse(string message)
        {
            if (this.streamResponse == null)
            {
                this.streamResponse = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(message),
                };
            }

            return this.streamResponse;
        }

        protected HttpResponseMessage GenerateAsyncOperationScheduled()
        {
            var operations = new ODataFeed<AsyncOperation>();
            operations.Feed = new List<AsyncOperation>
            {
                new AsyncOperation
                {
                    BatchID = GetId(10),
                    BatchSourceID = GetId(),
                    BatchProgress = 0,
                    BatchState = AsyncOperationState.Scheduled
                }
            };

            return new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                RequestMessage = new HttpRequestMessage(HttpMethod.Post, new Uri("https://secure.sf-api.com/sf/v3/")),
                Content = new StringContent(SerializeObject(operations), Encoding.UTF8, "application/json")
            };
        }

        protected HttpResponseMessage GenerateRedirectionResponse(string root = null)
        {
            var baseUri = "https://newhost.sharefile.com/sf/v3/";
            var redirection = new Redirection
            {
                Available = true,
                Uri = new Uri(baseUri),
                Root = root
            };

            redirection.MetadataUrl = "https://newhost.sharefile.com/sf/v3/$metadata#ShareFile.Api.Models.Redirection@Element";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SerializeObject(redirection), Encoding.UTF8, "application/json"),
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("https://secure.sf-api.com/sf/v3/Items(" + GetId() + ")"))
            };
        }

        protected HttpResponseMessage GenerateRedirectionUnavailableResponse()
        {
            var redirection = new Redirection
            {
                Uri = null
            };

            redirection.MetadataUrl = "https://newhost.sharefile.com/sf/v3/$metadata#ShareFile.Api.Models.Redirection@Element";

            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SerializeObject(redirection), Encoding.UTF8, "application/json"),
                RequestMessage = new HttpRequestMessage(HttpMethod.Get, new Uri("https://secure.sf-api.com/sf/v3/Items(" + GetId() + ")"))
            };

        }

        protected HttpResponseMessage GenerateItemResponse(HttpRequestMessage requestMessage)
        {
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(SerializeObject(new 
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
                Content = new StringContent(SerializeObject(new ODataRequestException
                {
                    Code = HttpStatusCode.Unauthorized,
                    Message = new ODataExceptionMessage { Language = "en-US", Message = "Unauthorized" }
                }), Encoding.UTF8, "application/json"),
                RequestMessage = requestMessage
            };

            responseMessage.Headers.WwwAuthenticate.Add(new AuthenticationHeaderValue("Bearer"));

            return responseMessage;
        }

        protected HttpResponseMessage GenerateNoContentResponse(HttpRequestMessage requestMessage)
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.NoContent)
            {
                RequestMessage = requestMessage
            };

            return responseMessage;
        }

        protected string SerializeObject(object obj)
        {
            var serializer = GetSerializer();

            var writer = new StringWriter();
            serializer.Serialize(writer, obj);

            return writer.ToString();
        }
    }
}
