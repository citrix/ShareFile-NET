using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Events;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Requests.Executors;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Client.Models;
using ShareFile.Api.Client.Security.Cryptography;

namespace ShareFile.Api.Client.Requests.Providers
{
	internal class AsyncRequestProvider : BaseRequestProvider, IAsyncRequestProvider
    {
        public AsyncRequestProvider(ShareFileClient client) : base(client) { }

        [NotNull]
        public async Task ExecuteAsync(IQuery query, CancellationToken token = default(CancellationToken))
        {
            EventHandlerResponse action = null;
            int retryCount = 0;

            do
            {
                var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging, RequestId);

                var apiRequest = ApiRequest.FromQuery(query as QueryBase, RequestId);
                if (action != null && action.Redirection != null)
                {
                    apiRequest.IsComposed = true;
                    apiRequest.Uri = action.Redirection.GetCalculatedUri();
                    apiRequest.Body = action.Redirection.Body;
                    apiRequest.HttpMethod = action.Redirection.Method ?? "GET";
                }

                var httpRequestMessage = BuildRequest(apiRequest);
                var responseMessage = await ExecuteRequestAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead, token: token).ConfigureAwait(false);

                action = null;

                try
                {
                    var response = await HandleResponse(responseMessage, apiRequest, retryCount++).ConfigureAwait(false);
                    if (response.Action != null)
                    {
                        action = response.Action;
                    }
                }
                finally
                {
                    ShareFileClient.Logging.Trace(requestRoundtripWatch);
                }
            } while (action != null && (action.Action == EventHandlerResponseAction.Retry || action.Action == EventHandlerResponseAction.Redirect));
        }

        [NotNull]
        public async Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken token = default(CancellationToken))
            where T : class
        {
            var streamQuery = query as IQuery<Stream>;
            if (streamQuery != null)
            {
                return (T)(object)(await ExecuteAsync(streamQuery, token).ConfigureAwait(false));
            }
            
            EventHandlerResponse action = null;
            int retryCount = 0;

            do
            {
                var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging, RequestId);

                var apiRequest = ApiRequest.FromQuery(query as QueryBase, RequestId);

                if (action != null && action.Redirection != null)
                {
                    apiRequest.IsComposed = true;
                    apiRequest.Uri = action.Redirection.GetCalculatedUri();
                    apiRequest.Body = action.Redirection.Body;
                    apiRequest.HttpMethod = action.Redirection.Method ?? "GET";
                }

                var httpRequestMessage = BuildRequest(apiRequest);

                var responseMessage = await ExecuteRequestAsync(httpRequestMessage, GetCompletionOptionForQuery(typeof(T)), token: token).ConfigureAwait(false);

                action = null;

                try
                {
                    if (typeof(T).IsSubclassOf(typeof(ODataObject)))
                    {
                        var response = await HandleTypedResponse<ODataObject>(responseMessage, apiRequest, retryCount++).ConfigureAwait(false);

                        if (response.Value != null)
                        {
                            string redirectUri;
                            if (response.Value is ODataObject && response.Value.TryGetProperty("Uri", out redirectUri))
                            {
                                var redirect = new Redirection
                                {
                                    Uri = new Uri(redirectUri)
                                };

                                string redirectRoot;
                                if(response.Value.TryGetProperty("Root", out redirectRoot))
                                {
                                    redirect.Root = redirectRoot;
                                }

                                response.Value = redirect;
                            }

                            if (response.Value is Redirection && typeof(T) != typeof(Redirection))
                            {
                                action = GetRedirectionAction(response, responseMessage, httpRequestMessage);
                            }
                            else if (response.Value is AsyncOperation && typeof(T) != typeof(AsyncOperation))
                            {
                                throw new AsyncOperationScheduledException(
                                    new ODataFeed<AsyncOperation>() { Feed = new[] { (AsyncOperation)response.Value } });
                            }
                            else
                            {
                                return (T)(object)response.Value;
                            }
                        }
                        else action = response.Action;
                    }
                    else
                    {
                        var response = await HandleTypedResponse<T>(responseMessage, apiRequest, retryCount++).ConfigureAwait(false);
                        if (response.Value != null)
                        {
                            return response.Value;
                        }
                        else action = response.Action;
                    }
                }
                finally
                {
                    ShareFileClient.Logging.Trace(requestRoundtripWatch);
                }

            } while (action != null && (action.Action == EventHandlerResponseAction.Retry || action.Action == EventHandlerResponseAction.Redirect));

            ShareFileClient.Logging.Error($"App should throw or return before getting to this point. Query: {query}");
            throw new InvalidOperationException("We should throw or return before getting here.");
        }

        private EventHandlerResponse GetRedirectionAction(
            Response<ODataObject> response,
            HttpResponseMessage responseMessage,
            HttpRequestMessage httpRequestMessage)
        {
            EventHandlerResponse action;
            var redirection = response.Value as Redirection;

            // Removed until API is updated to provide this correctly.
            // !redirection.Available || 
            if (redirection.Uri == null)
            {
                throw new ZoneUnavailableException(responseMessage.RequestMessage.RequestUri, "Destination zone is unavailable");
            }

            if (httpRequestMessage.RequestUri.GetAuthority() != redirection.Uri.GetAuthority())
            {
                action = this.ShareFileClient.OnChangeDomain(httpRequestMessage, redirection);
            }
            else
            {
                action = EventHandlerResponse.Redirect(redirection);
            }
            return action;
        }

        [NotNull]
        public async Task<T> ExecuteAsync<T>(IFormQuery<T> query, CancellationToken token = default(CancellationToken))
            where T : class
        {
            return await ExecuteAsync(query as IQuery<T>, token).ConfigureAwait(false);
        }

        [NotNull]
        public async Task<Stream> ExecuteAsync(
            IQuery<Stream> query,
            CancellationToken token = default(CancellationToken))
        {
            EventHandlerResponse action = null;
            int retryCount = 0;

            do
            {
                var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging, RequestId);

                var apiRequest = ApiRequest.FromQuery(query as QueryBase, RequestId);
                if (action != null && action.Redirection != null)
                {
                    apiRequest.IsComposed = true;
                    apiRequest.Uri = action.Redirection.GetCalculatedUri();
                    apiRequest.Body = action.Redirection.Body;
                    apiRequest.HttpMethod = action.Redirection.Method ?? "GET";
                }

                action = null;

                var httpRequestMessage = BuildRequest(apiRequest);
                var responseMessage = await ExecuteRequestAsync(httpRequestMessage, GetCompletionOptionForQuery(typeof(Stream)), token: token).ConfigureAwait(false);

                try
                {
                    var response = await HandleStreamResponse(responseMessage, apiRequest, retryCount++).ConfigureAwait(false);
                    if (response.Value != null) return response.Value;
                    else action = response.Action;
                }
                finally
                {
                    ShareFileClient.Logging.Trace(requestRoundtripWatch);
                }
            }
            while (action != null && (action.Action == EventHandlerResponseAction.Retry || action.Action == EventHandlerResponseAction.Redirect));

            ShareFileClient.Logging.Error($"App should throw or return before getting to this point. Query: {query}");
            throw new InvalidOperationException("We should throw or return before getting here.");
        }

        [NotNull]
        public Task<Stream> ExecuteAsync(IStreamQuery query, CancellationToken token = default(CancellationToken))
        {
            return this.ExecuteAsync((IQuery<Stream>)query, token);
        }

        protected async Task<Response<T>> HandleTypedResponse<T>(HttpResponseMessage httpResponseMessage, ApiRequest request, int retryCount, bool tryResolveUnauthorizedChallenge = true)
        {
            await CheckAsyncOperationScheduled(httpResponseMessage).ConfigureAwait(false);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var watch = new ActionStopwatch("ProcessResponse", ShareFileClient.Logging, RequestId);

                var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                if (responseStream != null)
                {
                    var result = await DeserializeResponseStreamAsync<T>(responseStream, httpResponseMessage).ConfigureAwait(false);

                    LogResponseAsync(result, httpResponseMessage.RequestMessage.RequestUri,
                        httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode)
                        .Forget();

                    CheckAsyncOperationScheduled(result);

                    ShareFileClient.Logging.Trace(watch);
                    return Response.CreateSuccess(result);
                }

                ShareFileClient.Logging.Trace(watch);

                throw new InvalidApiResponseException(httpResponseMessage.StatusCode, "Unable to read response stream");
            }
            else
            {
                LogResponseAsync(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri,
                    httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode)
                    .Forget();
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && tryResolveUnauthorizedChallenge)
            {
                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequestAsync(request, authenticatedHttpRequestMessage.Headers.ToString()).Forget();

                    var requestTask = ExecuteRequestAsync(authenticatedHttpRequestMessage, GetCompletionOptionForQuery(typeof(T))).ConfigureAwait(false);
                    using (var authenticatedResponse = await requestTask)
                    {
                        if (authenticatedResponse.IsSuccessStatusCode)
                        {
                            return await HandleTypedResponse<T>(authenticatedResponse, request, retryCount, false).ConfigureAwait(false);
                        }

                        return Response.CreateAction<T>(await HandleNonSuccess(authenticatedResponse, retryCount, typeof(T)).ConfigureAwait(false));
                    }
                }
            }

            return Response.CreateAction<T>(await HandleNonSuccess(httpResponseMessage, retryCount, typeof(T)).ConfigureAwait(false));
        }

        protected async Task<Response> HandleResponse(HttpResponseMessage httpResponseMessage, ApiRequest request, int retryCount, bool tryResolveUnauthorizedChallenge = true)
        {
            await CheckAsyncOperationScheduled(httpResponseMessage).ConfigureAwait(false);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                if (httpResponseMessage.HasContent())
                {
                    var response = await HandleTypedResponse<ODataObject>(httpResponseMessage, request, retryCount).ConfigureAwait(false);
                    if (response.Value is Redirection)
                    {
                        var action = GetRedirectionAction(
                            response,
                            httpResponseMessage,
                            httpResponseMessage.RequestMessage);

                        return Response.CreateAction(action);
                    }
                }

                return Response.Success;
            }
            else
            {
                LogResponseAsync(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri,
                    httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode)
                    .Forget();
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && tryResolveUnauthorizedChallenge)
            {
                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                httpResponseMessage.Dispose();
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequestAsync(request, authenticatedHttpRequestMessage.Headers.ToString()).Forget();

                    var authenticatedResponse = await HttpClient.SendAsync(authenticatedHttpRequestMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                    if (authenticatedResponse.IsSuccessStatusCode)
                    {
                        await HandleResponse(authenticatedResponse, request, retryCount, false).ConfigureAwait(false);
                    }

                    return Response.CreateAction(await HandleNonSuccess(authenticatedResponse, retryCount).ConfigureAwait(false));
                }
            }

            return Response.CreateAction(await HandleNonSuccess(httpResponseMessage, retryCount).ConfigureAwait(false));
        }

        protected async Task<Response<Stream>> HandleStreamResponse(HttpResponseMessage httpResponseMessage, ApiRequest request, int retryCount, bool tryResolveUnauthorizedChallenge = true)
        {
            await CheckAsyncOperationScheduled(httpResponseMessage).ConfigureAwait(false);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return new Response<Stream>
                {
                    Value = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false)
                };
            }
            else
            {
                LogResponseAsync(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri,
                    httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode)
                    .Forget();
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && tryResolveUnauthorizedChallenge)
            {
                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                httpResponseMessage.Dispose();
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequestAsync(request, authenticatedHttpRequestMessage.Headers.ToString()).Forget();

                    var requestTask = HttpClient.SendAsync(authenticatedHttpRequestMessage, HttpCompletionOption.ResponseContentRead);
                    using (var authenticatedResponse = await requestTask.ConfigureAwait(false))
                    {
                        if (authenticatedResponse.IsSuccessStatusCode)
                        {
                            return await HandleStreamResponse(authenticatedResponse, request, retryCount, false).ConfigureAwait(false);
                        }

                        return new Response<Stream>
                        {
                            Action = await HandleNonSuccess(authenticatedResponse, retryCount, typeof(Stream)).ConfigureAwait(false)
                        };
                    }
                }
            }

            return new Response<Stream>
            {
                Action = await HandleNonSuccess(httpResponseMessage, retryCount, typeof(Stream)).ConfigureAwait(false)
            };
        }

        protected async Task CheckAsyncOperationScheduled(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode == HttpStatusCode.Accepted)
            {
                if (httpResponseMessage.Content != null &&
                    httpResponseMessage.Content.Headers.ContentType.ToString()
                        .IndexOf("application/json", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    if (responseStream != null)
                    {
                        var asyncOperation =
                            await DeserializeResponseStreamAsync<ODataFeed<AsyncOperation>>(responseStream, httpResponseMessage).ConfigureAwait(false);

                        throw new AsyncOperationScheduledException(asyncOperation);
                    }
                }
            }
        }

        async Task<EventHandlerResponse> HandleNonSuccess(HttpResponseMessage responseMessage, int retryCount, Type expectedType = null)
        {
            var action = ShareFileClient.OnException(responseMessage, retryCount);

            if (action != null && action.Action == EventHandlerResponseAction.Throw)
            {
                if (responseMessage.StatusCode == HttpStatusCode.RequestTimeout ||
                    responseMessage.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    var exception =
                        new HttpRequestException(string.Format("{0}\r\n{1}: Request timeout",
                            responseMessage.RequestMessage.RequestUri, responseMessage.StatusCode));
                    ShareFileClient.Logging.Error(exception, "", null);
                }

                if (responseMessage.Headers != null && responseMessage.Headers.WwwAuthenticate != null
                    && responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var exception =
                        new WebAuthenticationException(
                            "Authentication failed with status code: " + (int)responseMessage.StatusCode,
                            responseMessage.Headers.WwwAuthenticate.ToList());
                    exception.RequestUri = responseMessage.RequestMessage.RequestUri;
                    ShareFileClient.Logging.Error(exception, string.Empty, null);
                    throw exception;
                }

                if (responseMessage.Content != null && responseMessage.Content.Headers != null && responseMessage.Content.Headers.ContentLength == 0)
                {
                    var exception = new InvalidApiResponseException(responseMessage.StatusCode,
                        "Unable to retrieve HttpResponseMessage.Content");

                    ShareFileClient.Logging.Error(exception, string.Empty, null);
                    throw exception;
                }

                if (responseMessage.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
                {
                    var exception =
                        new ProxyAuthenticationException("ProxyAuthentication failed with status code: " +
                                                         (int)responseMessage.StatusCode);
                    ShareFileClient.Logging.Error(exception, string.Empty, null);
                    throw exception;
                }

                var rawError = await responseMessage.Content.ReadAsStringAsync().ConfigureAwait(false);

                Exception exceptionToThrow = null;

                if (expectedType == null || expectedType.IsAssignableFrom(typeof(ODataObject)) || expectedType.IsAssignableFrom(typeof(Stream)))
                {
                    ODataRequestException requestException;
                    if (TryDeserialize(rawError, out requestException))
                    {
                        exceptionToThrow = new ODataException
                        {
                            Code = requestException.Code,
                            ODataExceptionMessage = requestException.Message,
                            ExceptionReason = requestException.ExceptionReason,
                            ErrorLog = requestException.ErrorLog
                        };
                    }
                    else
                    {
                        exceptionToThrow = new ODataException
                        {
                            Code = responseMessage.StatusCode,
                            ODataExceptionMessage = new ODataExceptionMessage(),
                        };
                    }
                }
                else if (expectedType == typeof(OAuthToken))
                {
                    OAuthError oauthError;

                    if (TryDeserialize(rawError, out oauthError))
                    {
                        exceptionToThrow = new OAuthErrorException
                        {
                            Error = oauthError
                        };
                    }
                    else
                    {
                        exceptionToThrow = new OAuthErrorException
                        {
                            Error = new OAuthError(),
                        };
                    }
                }
                else
                {
                    var invalidResponseException = new InvalidApiResponseException(responseMessage.StatusCode,
                        rawError);
                    ShareFileClient.Logging.Error(invalidResponseException, "", null);
                    exceptionToThrow = invalidResponseException;
                }

                if (exceptionToThrow != null)
                {
                    throw exceptionToThrow;
                }
            }

            return action;
        }

        protected async Task<HttpResponseMessage> ExecuteRequestAsync(
            HttpRequestMessage requestMessage,
            HttpCompletionOption httpCompletionOption,
            int redirectionCount = 0,
            CancellationToken token = default(CancellationToken))
        {
            if (redirectionCount >= MaxAutomaticRedirections)
            {
                throw new HttpRequestException("Exceeded maximum number of allowed redirects.");
            }
            var requestExecutor = ShareFileClient.AsyncRequestExecutor ?? RequestExecutorFactory.GetAsyncRequestExecutor();

            var responseMessage = await requestExecutor.SendAsync(HttpClient, requestMessage, httpCompletionOption, token).ConfigureAwait(false);
            var redirect = responseMessage.GetSecureRedirect();
            if (redirect != null)
            {
                return await ExecuteRequestAsync(requestMessage.Clone(redirect), httpCompletionOption, ++redirectionCount, token).ConfigureAwait(false);
            }

            ProcessCookiesForRuntime(responseMessage);

            return responseMessage;
        }
    }
}