using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
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
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Requests.Providers
{
    internal class SyncRequestProvider : BaseRequestProvider, ISyncRequestProvider
    {
        public SyncRequestProvider(ShareFileClient client) : base(client) { }

        public void Execute(IQuery query)
        {
            Execute(query as QueryBase,
                responseMessage => Response.Success);
        }

        public T Execute<T>(IQuery<T> query) where T : class
        {
            var streamQuery = query as IQuery<Stream>;
            if (streamQuery != null)
            {
                return this.Execute(streamQuery) as T;
            }

            var response = Execute(query as QueryBase, ParseTypedResponse<T>);
            return response.As((Response<T> typedResponse) => typedResponse.Value);
        }

        public T Execute<T>(IFormQuery<T> query) where T : class
        {
            return Execute(query as IQuery<T>);
        }

        public Stream Execute(IQuery<Stream> query)
        {
            var response = Execute(query as QueryBase, responseMessage => Response.CreateSuccess(responseMessage.Content.ReadAsStreamAsync().WaitForTask()));
            return response.As((Response<Stream> streamResponse) => streamResponse.Value);
        }

        public Stream Execute(IStreamQuery query)
        {
            return Execute((IQuery<Stream>)query);
        }

        private Response<T> ParseTypedResponse<T>(HttpResponseMessage httpResponseMessage)
            where T : class
        {
            var watch = new ActionStopwatch("ProcessResponse", ShareFileClient.Logging, RequestId);

            var responseStream = httpResponseMessage.Content.ReadAsStreamAsync().WaitForTask();
            if (responseStream != null)
            {
                if (typeof(T).IsSubclassOf(typeof(ODataObject)))
                {
                    var result = DeserializeResponseStream<ODataObject>(responseStream, httpResponseMessage);

                    LogResponse(result, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);
                    ShareFileClient.Logging.Trace(watch);

                    CheckAsyncOperationScheduled(result);

                    //workaround for metadata not returning on Redirections
                    string redirectUri;
                    if (result is ODataObject && result.TryGetProperty("Uri", out redirectUri))
                    {
                        var redirect = new Redirection { Uri = new Uri(redirectUri) };
                        string redirectRoot;
                        if(result.TryGetProperty("Root", out redirectRoot))
                        {
                            redirect.Root = redirectRoot;
                        }
                        result = redirect;
                    }

                    if (result is Redirection && typeof(T) != typeof(Redirection))
                    {
                        var redirection = result as Redirection;
                        
                        // Removed until API is updated to provide this correctly.
                        // !redirection.Available || 
                        if (redirection.Uri == null)
                            throw new ZoneUnavailableException(httpResponseMessage.RequestMessage.RequestUri, "Destination zone is unavailable");

                        if (httpResponseMessage.RequestMessage.RequestUri.GetAuthority() != redirection.Uri.GetAuthority())
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
                    else if (result is AsyncOperation)
                    {
                        throw new AsyncOperationScheduledException(
                            new ODataFeed<AsyncOperation>() { Feed = new[] { (AsyncOperation)result } });
                    }
                    else
                    {
                        throw new InvalidApiResponseException(httpResponseMessage.StatusCode, "Unable to parse API return to desired type");
                    }
                }
                else
                {
                    var result = DeserializeResponseStream<T>(responseStream, httpResponseMessage);
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
                var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging, RequestId);

                var apiRequest = ApiRequest.FromQuery(query, RequestId);

                if (action != null && action.Redirection != null)
                {
                    apiRequest.IsComposed = true;
                    apiRequest.Uri = action.Redirection.GetCalculatedUri();
                    apiRequest.Body = action.Redirection.Body;
                    apiRequest.HttpMethod = action.Redirection.Method ?? "GET";
                }

                var httpRequestMessage = BuildRequest(apiRequest);

                var responseMessage = ExecuteRequest(httpRequestMessage, GetCompletionOptionFromResponse(typeof(TResponse)));

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
                    ShareFileClient.Logging.Trace(requestRoundtripWatch);
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
                if (typeof(TResponse) == typeof(Response) && httpResponseMessage.HasContent())
                {
                    // It's weird to use Item here, however ODataObject goes down a different code path.
                    // Using Item reduces scope of changes.
                    return ParseTypedResponse<Item>(httpResponseMessage);
                }
                return parseSuccessResponse(httpResponseMessage);
            }
            else
            {
                LogResponse(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized && tryResolveUnauthorizedChallenge)
            {
                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequest(request, authenticatedHttpRequestMessage.Headers.ToString());

                    using (var authenticatedResponse = ExecuteRequest(authenticatedHttpRequestMessage, GetCompletionOptionFromResponse(typeof(TResponse))))
                    {
                        if (authenticatedResponse.IsSuccessStatusCode)
                        {
                            return HandleResponse(authenticatedResponse, parseSuccessResponse, request, retryCount, false);
                        }

                        return Response.CreateAction(HandleNonSuccess(authenticatedResponse, retryCount, typeof(TResponse)));
                    }
                }
            }

            return Response.CreateAction(HandleNonSuccess(httpResponseMessage, retryCount, typeof(TResponse)));
        }

        protected HttpResponseMessage ExecuteRequest(HttpRequestMessage requestMessage, HttpCompletionOption httpCompletionOption, int redirectionCount = 0)
        {
            if (redirectionCount >= MaxAutomaticRedirections)
            {
                throw new HttpRequestException("Exceeded maximum number of allowed redirects.");
            }
            var responseMessage = (ShareFileClient.SyncRequestExecutor ?? RequestExecutorFactory.GetSyncRequestExecutor())
                .Send(HttpClient, requestMessage, httpCompletionOption);

            var redirect = responseMessage.GetSecureRedirect();
            if (redirect != null)
            {
                return ExecuteRequest(requestMessage.Clone(redirect), httpCompletionOption, ++redirectionCount);
            }

            ProcessCookiesForRuntime(responseMessage);

            return responseMessage;
        }

        protected EventHandlerResponse HandleNonSuccess(HttpResponseMessage responseMessage, int retryCount, Type expectedType = null)
        {
            if (typeof(Response).IsAssignableFrom(expectedType))
            {
                var args = expectedType.GetGenericArguments();
                if (args != null && args.Length > 0)
                {
                    expectedType = args[0];
                }
                else
                {
                    expectedType = null;
                }
            }
            var action = ShareFileClient.OnException(responseMessage, retryCount);

            if (action != null && action.Action == EventHandlerResponseAction.Throw)
            {
                if (responseMessage.StatusCode == HttpStatusCode.RequestTimeout ||
                    responseMessage.StatusCode == HttpStatusCode.GatewayTimeout)
                {
                    var exception =
                        new HttpRequestException(string.Format("{0}\r\n{1}: Request timeout",
                            responseMessage.RequestMessage.RequestUri, responseMessage.StatusCode));
                    ShareFileClient.Logging.Error(exception, "[{0}]", new[] {RequestId});
                }

                if (responseMessage.Headers != null && responseMessage.Headers.WwwAuthenticate != null
                    && responseMessage.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var exception =
                        new WebAuthenticationException(
                            "Authentication failed with status code: " + (int)responseMessage.StatusCode,
                            responseMessage.Headers.WwwAuthenticate.ToList());
                    exception.RequestUri = responseMessage.RequestMessage.RequestUri;
                    ShareFileClient.Logging.Error(exception, "[{0}]", new[] { RequestId });
                    throw exception;
                }

                if (responseMessage.Content != null && responseMessage.Content.Headers != null && responseMessage.Content.Headers.ContentLength == 0)
                {
                    var exception = new NullReferenceException("Unable to retrieve HttpResponseMessage.Content");
                    ShareFileClient.Logging.Error(exception, "[{0}]", new[] { RequestId });
                    throw exception;
                }

                if (responseMessage.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
                {
                    var exception =
                        new ProxyAuthenticationException("ProxyAuthentication failed with status code: " +
                                                         (int)responseMessage.StatusCode);
                    ShareFileClient.Logging.Error(exception, "[{0}]", new[] { RequestId });
                    throw exception;
                }

                var rawError = responseMessage.Content.ReadAsStringAsync().WaitForTask();

                Exception exceptionToThrow = null;

                if (expectedType == null || expectedType.IsAssignableFrom(typeof(ODataObject)) || expectedType.IsSubclassOf(typeof(ODataObject)) || expectedType.IsAssignableFrom(typeof(Stream)))
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

        protected HttpCompletionOption GetCompletionOptionFromResponse(Type responseType)
        {
            if (responseType.IsGenericType() && responseType.IsGenericTypeOf(typeof(Response<>)) && responseType.GetGenericArguments().Length > 0)
                return GetCompletionOptionForQuery(responseType.GetGenericArguments()[0]);
            else
                return GetCompletionOptionForQuery(responseType);
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
                        var asyncOperation = DeserializeResponseStream<ODataFeed<AsyncOperation>>(responseStream, httpResponseMessage);

                        throw new AsyncOperationScheduledException(asyncOperation);
                    }
                }
            }
        }
    }
}
