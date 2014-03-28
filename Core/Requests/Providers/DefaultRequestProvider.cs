using System;
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
using ShareFile.Api.Client.Credentials;
using ShareFile.Api.Client.Events;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Requests.Providers
{
    public abstract class BaseRequestProvider
    {
        public ShareFileClient ShareFileClient { get; protected set; }

        protected BaseRequestProvider(ShareFileClient shareFileClient)
        {
            ShareFileClient = shareFileClient;
        }

        protected T DeserializeStream<T>(Stream stream)
        {
            var watch = new ActionStopwatch("DeserializeStream", ShareFileClient.Logging);

            T responseValue;

            using (var responseStreamReader = new StreamReader(stream))
            {
                JsonTextReader reader;
                if (ShareFileClient.Logging.IsDebugEnabled)
                {
                    var stringResponse = responseStreamReader.ReadToEnd();

                    reader = new JsonTextReader(new StringReader(stringResponse));
                    ShareFileClient.Logging.Debug(stringResponse, null);
                }
                else
                {
                    reader = new JsonTextReader(responseStreamReader);
                }

                responseValue = ShareFileClient.Serializer.Deserialize<T>(reader);
            }

            ShareFileClient.Logging.Trace(watch);

            return responseValue;
        }

        protected Task<T> DeserializeStreamAsync<T>(Stream stream)
        {
            var tcs = new TaskCompletionSource<T>();
            var task = tcs.Task;
            var watch = new ActionStopwatch("DeserializeStreamAsync", ShareFileClient.Logging);

            Task.Factory.StartNew(() =>
            {
                T responseValue;

                using (var responseStreamReader = new StreamReader(stream))
                {
                    JsonTextReader reader;
                    if (ShareFileClient.Logging.IsDebugEnabled)
                    {
                        var stringResponse = responseStreamReader.ReadToEnd();

                        reader = new JsonTextReader(new StringReader(stringResponse));
                        ShareFileClient.Logging.Debug(stringResponse, null);
                    }
                    else
                    {
                        reader = new JsonTextReader(responseStreamReader);
                    }

                    responseValue = ShareFileClient.Serializer.Deserialize<T>(reader);
                }


                ShareFileClient.Logging.Trace(watch);
                tcs.SetResult(responseValue);

            }).ConfigureAwait(false);

            return task;
        }

        protected string SerializeObject(object obj)
        {
            try
            {
                using (var stringWriter = new StringWriter())
                using (var textWriter = new JsonTextWriter(stringWriter))
                {
                    ShareFileClient.Serializer.Serialize(textWriter, obj);

                    return stringWriter.ToString();
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        protected void LogRequest(ApiRequest request, string headers)
        {
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                ShareFileClient.Logging.Debug("{0} {1}", request.HttpMethod, request.GetComposedUri());
                ShareFileClient.Logging.Debug("Headers: {0}", headers);
                LogCookies(request.GetComposedUri());
                if (request.Body != null)
                {
                    ShareFileClient.Logging.Debug("Content:{0}{1}", Environment.NewLine, SerializeObject(request.Body));
                }
            }
            else if (ShareFileClient.Logging.IsTraceEnabled)
            {
                ShareFileClient.Logging.Trace("{0} {1}", request.HttpMethod, request.GetComposedUri());
            }
        }

        protected void LogResponse<T>(T response, Uri requestUri, string headers, HttpStatusCode statusCode, MediaTypeHeaderValue mediaType = null)
        {
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                ShareFileClient.Logging.Debug("Response Code: {0}", statusCode);
                ShareFileClient.Logging.Debug("{0}", headers);
                LogCookies(requestUri);
                if (response != null)
                {
                    if (mediaType == null || mediaType.MediaType == "application/json")
                    {
                        ShareFileClient.Logging.Debug("Content:{0}{1}", Environment.NewLine, SerializeObject(response));
                    }
                    else ShareFileClient.Logging.Debug("Content {0}", response.ToString());
                }
            }
            else if (ShareFileClient.Logging.IsTraceEnabled)
            {
                ShareFileClient.Logging.Trace("Response Code: {0}", statusCode);
            }
        }

        protected void LogCookies(Uri uri)
        {
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                var cookieCollection = ShareFileClient.CookieContainer.GetCookies(uri);
                var stringBuilder = new StringBuilder("Cookies: ");

                foreach (var cookie in cookieCollection.Cast<Cookie>())
                {
                    stringBuilder.AppendFormat("{0}={1}; path={2}; domain={3}; expires={4};{5}{6}", cookie.Name,
                                               cookie.Value, cookie.Path, cookie.Domain, cookie.Expires,
                                               cookie.Secure ? " secure;" : "", cookie.HttpOnly ? " HttpOnly" : "");
                    stringBuilder.AppendLine();
                }

                ShareFileClient.Logging.Debug(stringBuilder.ToString());
            }
        }

        protected void WriteRequestBody(HttpRequestMessage httpRequestMessage, object body, MediaTypeHeaderValue contentType)
        {
            if (body is string)
            {
                using (var stringWriter = new StringWriter())
                {
                    stringWriter.Write(body as string);

                    if (ShareFileClient.Logging.IsDebugEnabled)
                    {
                        ShareFileClient.Logging.Debug(stringWriter.ToString(), null);
                    }

                    httpRequestMessage.Content = new StringContent(stringWriter.ToString());

                    if (httpRequestMessage.Content.Headers.ContentLength > 0)
                        httpRequestMessage.Content.Headers.ContentType = contentType;
                }
            }
            else if (body is IDictionary<string,string>)
            {
                var formContent = new FormUrlEncodedContent(body as IDictionary<string, string>);

                if (ShareFileClient.Logging.IsDebugEnabled)
                {
                    var contentAsString = formContent.ReadAsStringAsync();

                    ShareFileClient.Logging.Debug(contentAsString.Result, null);
                }
                
                httpRequestMessage.Content = formContent;
            }
            else
            {
                using (var stringWriter = new StringWriter())
                using (var textWriter = new JsonTextWriter(stringWriter))
                {
                    var serializationWatch = new ActionStopwatch("SerializeRequest", ShareFileClient.Logging);
                    ShareFileClient.Serializer.Serialize(textWriter, body);
                    ShareFileClient.Logging.Trace(serializationWatch);

                    if (ShareFileClient.Logging.IsDebugEnabled)
                    {
                        ShareFileClient.Logging.Debug(stringWriter.ToString(), null);
                    }

                    httpRequestMessage.Content = new StringContent(stringWriter.ToString());

                    if (httpRequestMessage.Content.Headers.ContentLength > 0)
                        httpRequestMessage.Content.Headers.ContentType = contentType;
                }
            }
        }

        protected AuthenticationHeaderValue GetAuthorizationHeaderValue(Uri requestUri, HttpHeaderValueCollection<AuthenticationHeaderValue> authenticationChallenge)
        {
            foreach (var wwwAuthenticateHeader in authenticationChallenge)
            {
                var networkCredential = ShareFileClient.CredentialCache.GetCredential(requestUri, wwwAuthenticateHeader.Scheme);

                if (networkCredential == null) continue;

                var oauthCredential = networkCredential as OAuth2Credential;
                if (oauthCredential != null)
                {
                    return new AuthenticationHeaderValue(wwwAuthenticateHeader.Scheme, oauthCredential.Password);
                }
            }

            return null;
        }

        protected AuthenticationHeaderValue GetAuthorizationHeaderValue(Uri requestUri, string scheme)
        {
            var networkCredential = ShareFileClient.CredentialCache.GetCredential(requestUri, scheme);

            if (networkCredential == null) return null;

            var oauthCredential = networkCredential as OAuth2Credential;
            if (oauthCredential != null)
            {
                return new AuthenticationHeaderValue(scheme, oauthCredential.Password);
            }

            return null;
        }

        protected bool TryDeserialize<T>(string rawException, out T exception)
        {
            try
            {
                using (var textReader = new JsonTextReader(new StringReader(rawException)))
                {
                    exception = ShareFileClient.Serializer.Deserialize<T>(textReader);
                }
            }
            catch
            {
                exception = default(T);
            }
            
            return exception != null;
        }
    }

    public class DefaultRequestProvider : BaseRequestProvider, IAsyncRequestProvider, ISyncRequestProvider
    {
        /// <summary>
        /// Set this flag True if running as a Portable Class Library
        /// </summary>
#if Portable
        public static bool RuntimeRequiresCustomCookieHandling = true;
#else
        public static bool RuntimeRequiresCustomCookieHandling = false;
#endif


        public HttpClient HttpClient { get; set; }

        public DefaultRequestProvider(ShareFileClient shareFileClient)
            : base (shareFileClient)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = false,
                Credentials = shareFileClient.CredentialCache,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
            };

            if (!RuntimeRequiresCustomCookieHandling)
            {
                handler.UseCookies = true;
                handler.CookieContainer = shareFileClient.CookieContainer;
            }
            else
            {
                handler.UseCookies = false;
            }

            if (shareFileClient.Configuration.ProxyConfiguration != null && handler.SupportsProxy)
            {
                handler.Proxy = shareFileClient.Configuration.ProxyConfiguration;
            }

            HttpClient = new HttpClient(handler, false)
            {
                Timeout = new TimeSpan(0, 0, 0, 0, shareFileClient.Configuration.HttpTimeout)
            };
        }

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
                var responseMessage = await ExecuteRequestAsync(httpRequestMessage, token);

                action = null;

                try
                {
                    var response = await HandleResponse(responseMessage, apiRequest, retryCount++);
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
                var responseMessage = await ExecuteRequestAsync(httpRequestMessage, token);

                action = null;

                try
                {
                    if (typeof (T).IsSubclassOf(typeof (ODataObject)))
                    {
                        var response = await HandleTypedResponse<ODataObject>(responseMessage, apiRequest, retryCount++);

                        if (response.Value != null)
                        {
                            if (response.Value is Redirection)
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
                        var response = await HandleTypedResponse<T>(responseMessage, apiRequest, retryCount++);
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
            return await ExecuteAsync(query as IQuery<T>, token);
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
                var responseMessage = await ExecuteRequestAsync(httpRequestMessage, token);

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

        public void Execute(IQuery query)
        {
            try
            {
                ExecuteAsync(query).Wait();
            }
            catch (AggregateException aggregateException)
            {
                throw aggregateException.GetBaseException();
            }
        }

        public T Execute<T>(IQuery<T> query)
            where T : class
        {
            try
            {
                var task = ExecuteAsync(query);
                return task.Result;
            }
            catch (AggregateException aggregateException)
            {
                throw aggregateException.GetBaseException();
            }
        }

        public T Execute<T>(IFormQuery<T> query)
            where T : class
        {
            try
            {
                var task = ExecuteAsync(query);
                return task.Result;
            }
            catch (AggregateException aggregateException)
            {
                throw aggregateException.GetBaseException();
            }
        }

        public Stream Execute(IStreamQuery query)
        {
            try
            {
                var task = ExecuteAsync(query);
                return task.Result;
            }
            catch (AggregateException aggregateException)
            {
                throw aggregateException.GetBaseException();
            }
        }

        protected HttpRequestMessage BuildRequest(ApiRequest request)
        {
            HttpRequestMessage requestMessage;
            var watch = new ActionStopwatch("BuildRequest", ShareFileClient.Logging);
            var uri = request.GetComposedUri();

#if ShareFile
            if (ShareFileClient.ZoneAuthentication != null) uri = ShareFileClient.ZoneAuthentication.Sign(uri);
#endif
            
            if (ShareFileClient.Configuration.UseHttpMethodOverride)
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
                requestMessage.Headers.Add("X-Http-Method-Override", request.HttpMethod);
            }
            else
            {
                if (request.HttpMethod == "GET" && request.Body != null)
                {
                    request.HttpMethod = "POST";
                }

                requestMessage = new HttpRequestMessage(new HttpMethod(request.HttpMethod), uri);
            }

            foreach (var kvp in request.HeaderCollection)
            {
                requestMessage.Headers.Add(kvp.Key, kvp.Value);
            }

            if (request.Body != null)
            {
                requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                WriteRequestBody(requestMessage, request.Body, new MediaTypeHeaderValue("application/json"));
            }
            else
            {
                if (requestMessage.Method == HttpMethod.Post)
                {
                    requestMessage.Content = new StringContent("");
                }
            }

            if (RuntimeRequiresCustomCookieHandling)
            {
                var cookieHeader = ShareFileClient.CookieContainer.GetCookieHeader(
                    new Uri("https://www." + requestMessage.RequestUri.Host + requestMessage.RequestUri.AbsolutePath));

                if (!string.IsNullOrWhiteSpace(cookieHeader))
                {
                    requestMessage.Headers.Add("Cookie", cookieHeader);
                }
            }

            ShareFileClient.Logging.Trace(watch);

            return requestMessage;
        }

        protected async Task<Response<T>> HandleTypedResponse<T>(HttpResponseMessage httpResponseMessage, ApiRequest request, int retryCount, bool tryResolveUnauthorizedChallenge = true)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var watch = new ActionStopwatch("ProcessResponse", ShareFileClient.Logging);

                var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                if (responseStream != null)
                {
                    var result = await DeserializeStreamAsync<T>(responseStream);
                    
                    LogResponse(result, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);

                    ShareFileClient.Logging.Trace(watch);
                    return Response.CreateSuccess(result);
                }

                ShareFileClient.Logging.Trace(watch);

                throw new InvalidApiResponseException(httpResponseMessage.StatusCode, "Unable to read response stream");
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                LogResponse(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);

                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequest(request, authenticatedHttpRequestMessage.Headers.ToString());

                    var requestTask = ExecuteRequestAsync(authenticatedHttpRequestMessage);
                    using (var authenticatedResponse = await requestTask)
                    {
                        if (authenticatedResponse.IsSuccessStatusCode)
                        {
                            return await HandleTypedResponse<T>(authenticatedResponse, request, retryCount, false);
                        }

                        Response.CreateAction<T>(await HandleNonSuccess(authenticatedResponse, retryCount, typeof(T)));
                    }
                }
            }

            return Response.CreateAction<T>(await HandleNonSuccess(httpResponseMessage, retryCount, typeof(T)));
        }

        protected async Task<Response> HandleResponse(HttpResponseMessage httpResponseMessage, ApiRequest request, int retryCount, bool tryResolveUnauthorizedChallenge = true)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return Response.Success;
            }

            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                LogResponse(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);

                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                httpResponseMessage.Dispose();
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequest(request, authenticatedHttpRequestMessage.Headers.ToString());

                    var authenticatedResponse = await HttpClient.SendAsync(authenticatedHttpRequestMessage, HttpCompletionOption.ResponseContentRead);
                    
                    if (authenticatedResponse.IsSuccessStatusCode)
                    {
                        await HandleResponse(authenticatedResponse, request, retryCount, false);
                    }

                    return Response.CreateAction(await HandleNonSuccess(authenticatedResponse, retryCount));
                }
            }

            return Response.CreateAction(await HandleNonSuccess(httpResponseMessage, retryCount));
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
                LogResponse(httpResponseMessage, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);

                var authorizationHeaderValue = GetAuthorizationHeaderValue(httpResponseMessage.RequestMessage.RequestUri,
                                                                        httpResponseMessage.Headers.WwwAuthenticate);
                httpResponseMessage.Dispose();
                if (authorizationHeaderValue != null)
                {
                    var authenticatedHttpRequestMessage = BuildRequest(request);
                    authenticatedHttpRequestMessage.Headers.Authorization = authorizationHeaderValue;

                    LogRequest(request, authenticatedHttpRequestMessage.Headers.ToString());

                    var requestTask = HttpClient.SendAsync(authenticatedHttpRequestMessage, HttpCompletionOption.ResponseContentRead);
                    using (var authenticatedResponse = await requestTask)
                    {
                        if (authenticatedResponse.IsSuccessStatusCode)
                        {
                            return await HandleStreamResponse(authenticatedResponse, request, retryCount, false);
                        }

                        return new Response<Stream>
                        {
                            Action = await HandleNonSuccess(authenticatedResponse, retryCount, typeof(Stream))
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
                            "Authentication failed with status code: " + (int) responseMessage.StatusCode,
                            supportedSchemes);
                    exception.RequestUri = responseMessage.RequestMessage.RequestUri;
                    ShareFileClient.Logging.Error(exception, string.Empty, null);
                    throw exception;
                }

                if (responseMessage.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
                {
                    var exception =
                        new ProxyAuthenticationException("ProxyAuthentication failed with status code: " +
                                                         (int) responseMessage.StatusCode);
                    ShareFileClient.Logging.Error(exception, string.Empty, null);
                    throw exception;
                }

                var rawError = await responseMessage.Content.ReadAsStringAsync();
                
                Exception exceptionToThrow = null;
                        
                if (expectedType == null || expectedType.IsSubclassOf(typeof (ODataObject)))
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
                else if (expectedType == typeof (OAuthToken))
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

    public class Response<T> : Response
    {
        public T Value { get; set; }
    }

    public class Response
    {
        public EventHandlerResponse Action { get; set; }
        public static Response<TSuccess> CreateSuccess<TSuccess>(TSuccess value)
        {
            return new Response<TSuccess>
            {
                Value = value
            };
        }

        public static Response<TSuccess> CreateAction<TSuccess>(EventHandlerResponse response)
        {
            return new Response<TSuccess>
            {
                Action = response
            };
        }

        public static Response CreateAction(EventHandlerResponse response)
        {
            return new Response
            {
                Action = response
            };
        }

        public static Response Success = new Response();
    }
}
