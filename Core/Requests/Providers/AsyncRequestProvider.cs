using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Credentials;
using ShareFile.Api.Client.Events;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Requests.Providers
{
#if async
    internal class AsyncRequestProvider : BaseRequestProvider, IAsyncRequestProvider
    {
        public AsyncRequestProvider(ShareFileClient client) : base(client) { }

        public async Task ExecuteAsync(IQuery query, CancellationToken? token = null)
        {
            EventHandlerResponse action = null;
            int retryCount = 0;

            do
            {
                var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging);

                var apiRequest = ApiRequest.FromQuery(query as QueryBase);

                if (action != null && action.Redirection != null && action.Redirection.Body != null)
                {
                    apiRequest.IsComposed = true;
                    apiRequest.Uri = action.Redirection.Uri;
                    apiRequest.Body = action.Redirection.Body;
                    apiRequest.HttpMethod = action.Redirection.Method ?? "GET";
                }

                var httpRequestMessage = BuildRequest(apiRequest);
                var responseMessage = await ExecuteRequestAsync(httpRequestMessage, token).ConfigureAwait(false);

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
                    requestRoundtripWatch.Stop();
                }
            } while (action != null && (action.Action == EventHandlerResponseAction.Retry || action.Action == EventHandlerResponseAction.Redirect));
        }

        public async Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken? token = null)
            where T : class
        {
            EventHandlerResponse action = null;
            int retryCount = 0;

            do
            {
                var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging);

                var apiRequest = ApiRequest.FromQuery(query as QueryBase);

                if (action != null && action.Redirection != null && action.Redirection.Body != null)
                {
                    apiRequest.IsComposed = true;
                    apiRequest.Uri = action.Redirection.Uri;
                    apiRequest.Body = action.Redirection.Body;
                    apiRequest.HttpMethod = action.Redirection.Method ?? "GET";
                }

                var httpRequestMessage = BuildRequest(apiRequest);

                if (httpRequestMessage.RequestUri.AbsolutePath.Contains("Sessions/Login"))
                {
                    var authorizationHeader = GetAuthorizationHeaderValue(httpRequestMessage.RequestUri, "Bearer");

                    if (authorizationHeader != null)
                    {
                        httpRequestMessage.Headers.Authorization = authorizationHeader;
                        httpRequestMessage.Headers.Add("X-SFAPI-Tool", ShareFileClient.Configuration.ToolName);
                        httpRequestMessage.Headers.Add("X-SFAPI-ToolVersion", ShareFileClient.Configuration.ToolVersion);
                    }
                }

                var responseMessage = await ExecuteRequestAsync(httpRequestMessage, token);

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
                                response.Value = new Redirection
                                {
                                    Uri = new Uri(redirectUri)
                                };
                            }

                            if (response.Value is Redirection && typeof(T) != typeof(Redirection))
                            {
                                var redirection = response.Value as Redirection;
                                if (httpRequestMessage.RequestUri.GetAuthority() != redirection.Uri.GetAuthority())
                                {
                                    ShareFileClient.OnChangeDomain(httpRequestMessage, redirection);
                                }
                                action = EventHandlerResponse.Redirect(redirection);
                            }
                            else
                            {
                                return response.Value as T;
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
                    requestRoundtripWatch.Stop();
                }

            } while (action != null && (action.Action == EventHandlerResponseAction.Retry || action.Action == EventHandlerResponseAction.Redirect));

            return default(T);
        }

        public async Task<T> ExecuteAsync<T>(IFormQuery<T> query, CancellationToken? token = null)
            where T : class
        {
            return await ExecuteAsync(query as IQuery<T>, token).ConfigureAwait(false);
        }

        public async Task<Stream> ExecuteAsync(IStreamQuery query, CancellationToken? token = null)
        {
            EventHandlerResponse action = null;
            int retryCount = 0;

            do
            {
                var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging);

                var apiRequest = ApiRequest.FromQuery(query as QueryBase);
                if (action != null && action.Redirection != null && action.Redirection.Body != null)
                {
                    apiRequest.IsComposed = true;
                    apiRequest.Uri = action.Redirection.Uri;
                    apiRequest.Body = action.Redirection.Body;
                    apiRequest.HttpMethod = action.Redirection.Method ?? "GET";
                }

                action = null;

                var httpRequestMessage = BuildRequest(apiRequest);
                var responseMessage = await ExecuteRequestAsync(httpRequestMessage, token).ConfigureAwait(false);

                try
                {
                    var response = await HandleStreamResponse(responseMessage, apiRequest, retryCount++);
                    if (response.Value != null) return response.Value;
                    else action = response.Action;
                }
                finally
                {
                    requestRoundtripWatch.Stop();
                }

            } while (action != null && (action.Action == EventHandlerResponseAction.Retry || action.Action == EventHandlerResponseAction.Redirect));

            return default(Stream);
        }

        protected async Task<Response<T>> HandleTypedResponse<T>(HttpResponseMessage httpResponseMessage, ApiRequest request, int retryCount, bool tryResolveUnauthorizedChallenge = true)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var watch = new ActionStopwatch("ProcessResponse", ShareFileClient.Logging);

                var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                if (responseStream != null)
                {
                    var result = await DeserializeStreamAsync<T>(responseStream).ConfigureAwait(false);

                    LogResponseAsync(result, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode).ConfigureAwait(false);

                    ShareFileClient.Logging.Trace(watch);
                    return Response.CreateSuccess(result);
                }

                ShareFileClient.Logging.Trace(watch);

                throw new InvalidApiResponseException(httpResponseMessage.StatusCode, "Unable to read response stream");
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                LogResponseAsync(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode).ConfigureAwait(false);

                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequestAsync(request, authenticatedHttpRequestMessage.Headers.ToString()).ConfigureAwait(false);

                    var requestTask = ExecuteRequestAsync(authenticatedHttpRequestMessage).ConfigureAwait(false);
                    using (var authenticatedResponse = await requestTask)
                    {
                        if (authenticatedResponse.IsSuccessStatusCode)
                        {
                            return await HandleTypedResponse<T>(authenticatedResponse, request, retryCount, false);
                        }

                        Response.CreateAction<T>(await HandleNonSuccess(authenticatedResponse, retryCount, typeof(T)).ConfigureAwait(false));
                    }
                }
            }

            return Response.CreateAction<T>(await HandleNonSuccess(httpResponseMessage, retryCount, typeof(T)).ConfigureAwait(false));
        }

        protected async Task<Response> HandleResponse(HttpResponseMessage httpResponseMessage, ApiRequest request, int retryCount, bool tryResolveUnauthorizedChallenge = true)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return Response.Success;
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                LogResponseAsync(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode).ConfigureAwait(false);

                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                httpResponseMessage.Dispose();
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequestAsync(request, authenticatedHttpRequestMessage.Headers.ToString()).ConfigureAwait(false);

                    var authenticatedResponse = await HttpClient.SendAsync(authenticatedHttpRequestMessage, HttpCompletionOption.ResponseContentRead).ConfigureAwait(false);

                    if (authenticatedResponse.IsSuccessStatusCode)
                    {
                        await HandleResponse(authenticatedResponse, request, retryCount, false);
                    }

                    return Response.CreateAction(await HandleNonSuccess(authenticatedResponse, retryCount).ConfigureAwait(false));
                }
            }

            return Response.CreateAction(await HandleNonSuccess(httpResponseMessage, retryCount).ConfigureAwait(false));
        }

        protected async Task<Response<Stream>> HandleStreamResponse(HttpResponseMessage httpResponseMessage, ApiRequest request, int retryCount, bool tryResolveUnauthorizedChallenge = true)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return new Response<Stream>
                {
                    Value = await httpResponseMessage.Content.ReadAsStreamAsync()
                };
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                LogResponseAsync(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode).ConfigureAwait(false);

                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                httpResponseMessage.Dispose();
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequestAsync(request, authenticatedHttpRequestMessage.Headers.ToString()).ConfigureAwait(false);

                    var requestTask = HttpClient.SendAsync(authenticatedHttpRequestMessage, HttpCompletionOption.ResponseContentRead);
                    using (var authenticatedResponse = await requestTask)
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
                Action = await HandleNonSuccess(httpResponseMessage, retryCount, typeof(Stream))
            };
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

                if (responseMessage.Content.Headers.ContentLength == 0)
                {
                    var exception = new NullReferenceException("Unable to retrieve HttpResponseMessage.Content");
                    ShareFileClient.Logging.Error(exception, string.Empty, null);
                    throw exception;
                }

                if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var supportedSchemes = responseMessage.Headers.WwwAuthenticate.Select(x => x.Scheme).ToList();

                    var exception =
                        new WebAuthenticationException(
                            "Authentication failed with status code: " + (int)responseMessage.StatusCode,
                            supportedSchemes);
                    exception.RequestUri = responseMessage.RequestMessage.RequestUri;
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

                var rawError = await responseMessage.Content.ReadAsStringAsync();

                Exception exceptionToThrow = null;

                if (expectedType == null || expectedType.IsAssignableFrom(typeof(ODataObject)))
                {
                    ODataRequestException requestException;
                    if (TryDeserialize(rawError, out requestException))
                    {
                        exceptionToThrow = new ODataException
                        {
                            Code = requestException.Code,
                            ODataExceptionMessage = requestException.Message
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

        protected async Task<HttpResponseMessage> ExecuteRequestAsync(HttpRequestMessage requestMessage, CancellationToken? token = null)
        {
            HttpResponseMessage responseMessage;
            if (token == null)
            {
                responseMessage = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            }
            else responseMessage = await HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token.Value);

            if (RuntimeRequiresCustomCookieHandling)
            {
                IEnumerable<string> newCookies;
                if (responseMessage.Headers.TryGetValues("Set-Cookie", out newCookies))
                {
                    foreach (var newCookie in newCookies)
                    {
                        ShareFileClient.CookieContainer.SetCookies(responseMessage.RequestMessage.RequestUri, newCookie);
                    }
                }
            }

            return responseMessage;
        }
    }
#endif
}
