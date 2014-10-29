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
using ShareFile.Api.Client.Extensions.Tasks;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Requests.Executors;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Requests.Providers
{
    internal class SyncRequestProvider : BaseRequestProvider, ISyncRequestProvider
    {
        public SyncRequestProvider(ShareFileClient client) : base(client) { }

        public void Execute(IQuery query)
        {
            Execute(query as QueryBase, responseMessage => Response.Success);
        }

        public T Execute<T>(IQuery<T> query) where T : class
        {
            var response = Execute(query as QueryBase, responseMessage => ParseTypedResponse<T>(responseMessage));
            return response.As(
                (Response<T> typedResponse) => typedResponse.Value,
                default(T));
        }

        public T Execute<T>(IFormQuery<T> query) where T : class
        {
            return Execute(query as IQuery<T>);
        }

        public Stream Execute(IStreamQuery query)
        {
            var response = Execute(query as QueryBase, responseMessage => Response.CreateSuccess(responseMessage.Content.ReadAsStreamAsync().WaitForTask()));
            return response.As(
                (Response<Stream> streamResponse) => streamResponse.Value,
                default(Stream));
        }

        private Response<T> ParseTypedResponse<T>(HttpResponseMessage httpResponseMessage)
            where T : class
        {
            var watch = new ActionStopwatch("ProcessResponse", ShareFileClient.Logging);

            var responseStream = httpResponseMessage.Content.ReadAsStreamAsync().WaitForTask();
            if (responseStream != null)
            {
                if (typeof(T).IsSubclassOf(typeof(ODataObject)))
                {
                    var result = DeserializeStream<ODataObject>(responseStream);
                    LogResponse(result, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);
                    ShareFileClient.Logging.Trace(watch);

                    //workaround for metadata not returning on Redirections
                    string redirectUri;
                    if (result is ODataObject && result.TryGetProperty("Uri", out redirectUri))
                    {
                        result = new Redirection { Uri = new Uri(redirectUri) };
                    }

                    if (result is Redirection && typeof(T) != typeof(Redirection))
                    {
                        var redirection = result as Redirection;

                        if(!redirection.Available || redirection.Uri == null)
                        {
                            throw new ZoneUnavailableException(httpResponseMessage.RequestMessage.RequestUri, "Destination zone is unavailable");
                        }
                        else if (httpResponseMessage.RequestMessage.RequestUri.GetAuthority() != redirection.Uri.GetAuthority())
                        {
                            return Response.CreateAction<T>(ShareFileClient.OnChangeDomain(httpResponseMessage.RequestMessage, redirection));
                        }
                        else
                        {
                            return Response.CreateAction<T>(EventHandlerResponse.Redirect(redirection));
                        }
                    }
                    else if(result is T)
                    {
                        return Response.CreateSuccess(result as T);
                    }
                    else
                    {
                        throw new InvalidApiResponseException(httpResponseMessage.StatusCode, "Unable to parse API return to desired type");
                    }
                }
                else
                {
                    var result = DeserializeStream<T>(responseStream);
                    LogResponse(result, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);
                    ShareFileClient.Logging.Trace(watch);

                    return Response.CreateSuccess(result);
                }
            }

            ShareFileClient.Logging.Trace(watch);

            throw new InvalidApiResponseException(httpResponseMessage.StatusCode, "Unable to read response stream");
        }

        private Response Execute<TResponse>(QueryBase query, Func<HttpResponseMessage, TResponse> parseSuccessResponse) 
            where TResponse : Response
        {
            EventHandlerResponse action = null;
            int retryCount = 0;

            do
            {
                var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging);

                var apiRequest = ApiRequest.FromQuery(query);

                if (action != null && action.Redirection != null)
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

                var responseMessage = ExecuteRequest(httpRequestMessage);

                action = null;

                try
                {
                    var response = HandleResponse(responseMessage, parseSuccessResponse, apiRequest, retryCount++);
                    if (response.Action != null)
                    {
                        action = response.Action;
                    }
                    else
                    {
                        return response;
                    }
                }
                finally
                {
                    requestRoundtripWatch.Stop();
                }

            } while (action != null && (action.Action == EventHandlerResponseAction.Retry || action.Action == EventHandlerResponseAction.Redirect));

            return Response.CreateAction(action); //when do we get here? ignore?
        }

        private Response HandleResponse<TResponse>(HttpResponseMessage httpResponseMessage, Func<HttpResponseMessage, TResponse> parseSuccessResponse, ApiRequest request, int retryCount, bool tryResolveUnauthorizedChallenge = true)
            where TResponse : Response
        {
            CheckAsyncOperationScheduled(httpResponseMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return parseSuccessResponse(httpResponseMessage);
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && tryResolveUnauthorizedChallenge)
            {
                LogResponse(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);

                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequest(request, authenticatedHttpRequestMessage.Headers.ToString());

                    using (var authenticatedResponse = ExecuteRequest(authenticatedHttpRequestMessage))
                    {
                        if (authenticatedResponse.IsSuccessStatusCode)
                        {
                            return HandleResponse(authenticatedResponse, parseSuccessResponse, request, retryCount, false);
                        }

                        return Response.CreateAction(HandleNonSuccess(authenticatedResponse, retryCount));
                    }
                }
            }

            return Response.CreateAction(HandleNonSuccess(httpResponseMessage, retryCount));
        }

        protected HttpResponseMessage ExecuteRequest(HttpRequestMessage requestMessage)
        {
            var responseMessage = RequestExecutorFactory.GetSyncRequestExecutor()
                .Send(HttpClient, requestMessage, HttpCompletionOption.ResponseContentRead);

            ProcessCookiesForRuntime(responseMessage);

            return responseMessage;
        }

        protected EventHandlerResponse HandleNonSuccess(HttpResponseMessage responseMessage, int retryCount, Type expectedType = null)
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

                if (responseMessage.Content != null && responseMessage.Content.Headers != null && responseMessage.Content.Headers.ContentLength == 0)
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

                var rawError = responseMessage.Content.ReadAsStringAsync().WaitForTask();

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

        protected void CheckAsyncOperationScheduled(HttpResponseMessage httpResponseMessage)
        {
            if (httpResponseMessage.StatusCode == HttpStatusCode.Accepted)
            {
                if (httpResponseMessage.Content != null &&
                    httpResponseMessage.Content.Headers.ContentType.ToString()
                        .IndexOf("application/json", StringComparison.OrdinalIgnoreCase) > -1)
                {
                    var responseStream = httpResponseMessage.Content.ReadAsStreamAsync().WaitForTask();
                    if (responseStream != null)
                    {
                        var asyncOperation = DeserializeStream<AsyncOperation>(responseStream);

                        throw new AsyncOperationScheduledException(asyncOperation);
                    }
                }
            }
        }
    }
}
