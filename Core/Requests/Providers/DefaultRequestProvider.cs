using System;
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
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Logging;

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

        protected T DeserializeStreamAsync<T>(Stream stream)
        {
            var watch = new ActionStopwatch("DeserializeStreamAsync", ShareFileClient.Logging);

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

        protected void LogResponse<T>(T response, Uri requestUri, string headers, HttpStatusCode statusCode)
        {
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                ShareFileClient.Logging.Debug("Response Code: {0}", statusCode);
                ShareFileClient.Logging.Debug("{0}", headers);
                LogCookies(requestUri);
                if (response != null)
                {
                    ShareFileClient.Logging.Debug("Content:{0}{1}", Environment.NewLine, SerializeObject(response));
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
    }

    public class DefaultRequestProvider : BaseRequestProvider, IAsyncRequestProvider, ISyncRequestProvider
    {
        public HttpClient HttpClient { get; set; }

        public DefaultRequestProvider(ShareFileClient shareFileClient)
            : base (shareFileClient)
        {
            var handler = new HttpClientHandler
            {
                AllowAutoRedirect = true,
                Credentials = shareFileClient.CredentialCache,
                AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                UseCookies = true
            };

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
            var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging);

            var apiRequest = ApiRequest.FromQuery(query as QueryBase);
            var httpRequestMessage = BuildRequest(apiRequest);
            var responseMessage = await ExecuteRequestAsync(httpRequestMessage, token);

            try
            {
                await HandleResponse(responseMessage, apiRequest);
            }
            finally
            {
                requestRoundtripWatch.Stop();
            }
        }
        //var watch = new ActionStopwatch("ExecuteRequest", ShareFileClient.Logging);
        public async Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken? token = null)
        {
            var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging);

            var apiRequest = ApiRequest.FromQuery(query as QueryBase);
            var httpRequestMessage = BuildRequest(apiRequest);
            var responseMessage = await ExecuteRequestAsync(httpRequestMessage, token);

            try
            {
                return await HandleTypedResponse<T>(responseMessage, apiRequest);
            }
            finally
            {
                requestRoundtripWatch.Stop();
            }
        }

        public async Task<Stream> ExecuteAsync(IStreamQuery query, CancellationToken? token = null)
        {
            var requestRoundtripWatch = new ActionStopwatch("RequestRoundTrip", ShareFileClient.Logging);

            var apiRequest = ApiRequest.FromQuery(query as QueryBase);
            var httpRequestMessage = BuildRequest(apiRequest);
            var responseMessage = await ExecuteRequestAsync(httpRequestMessage, token);

            try
            {
                return await HandleStreamResponse(responseMessage, apiRequest);
            }
            finally
            {
                requestRoundtripWatch.Stop();
            }
        }

        public void Execute(IQuery query)
        {
            var executeTask = ExecuteAsync(query);
            executeTask.Wait();
        }

        public T Execute<T>(IQuery<T> query)
        {
            var executeTask = ExecuteAsync(query);
            executeTask.Wait();

            return executeTask.Result;
        }

        public Stream Execute(IStreamQuery query)
        {
            var executeTask = ExecuteAsync(query);
            executeTask.Wait();

            return executeTask.Result;
        }

        public HttpRequestMessage BuildRequest(ApiRequest request)
        {
            HttpRequestMessage requestMessage;
            var watch = new ActionStopwatch("BuildRequest", ShareFileClient.Logging);
            var uri = request.GetComposedUri();

            //if (ShareFileClient.ZoneAuthentication != null) uri = ShareFileClient.ZoneAuthentication.Sign(uri);
            if (ShareFileClient.Configuration.UseHttpMethodOverride)
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
                requestMessage.Headers.Add("X-Http-Method-Override", request.HttpMethod);
            }
            else
            {
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

            ShareFileClient.Logging.Trace(watch);

            return requestMessage;
        }

        protected async Task<T> HandleTypedResponse<T>(HttpResponseMessage httpResponseMessage, ApiRequest request, bool tryResolveUnauthorizedChallenge = true)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                var watch = new ActionStopwatch("ProcessResponse", ShareFileClient.Logging);
#if PORTABLE
            ProcessHeaders(response);
#endif
                var responseStream = await httpResponseMessage.Content.ReadAsStreamAsync();
                if (responseStream != null)
                {
                    var result = DeserializeStreamAsync<T>(responseStream);
                    //ComposeResult(result);

                    //result.SetRequestUri(httpResponseMessage.RequestMessage);

                    LogResponse(result, httpResponseMessage.RequestMessage.RequestUri, httpResponseMessage.Headers.ToString(), httpResponseMessage.StatusCode);

                    ShareFileClient.Logging.Trace(watch);
                    return result;
                }

                ShareFileClient.Logging.Trace(watch);

                throw new InvalidApiResponseException(httpResponseMessage.StatusCode, "Unable to read response stream");
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
                            return await HandleTypedResponse<T>(authenticatedResponse, request, false);
                        }
                        
                        await HandleNonSuccess(authenticatedResponse);
                        return default(T);
                    }
                }
            }

            await HandleNonSuccess(httpResponseMessage);

            return default(T);
        }

        protected async Task HandleResponse(HttpResponseMessage httpResponseMessage, ApiRequest request, bool tryResolveUnauthorizedChallenge = true)
        {
            if (httpResponseMessage.IsSuccessStatusCode) return;

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
                            await HandleResponse(authenticatedResponse, request, false);
                        }

                        await HandleNonSuccess(authenticatedResponse);
                        return;
                    }
                }
            }

            await HandleNonSuccess(httpResponseMessage);
        }

        protected async Task<Stream> HandleStreamResponse(HttpResponseMessage httpResponseMessage, ApiRequest request, bool tryResolveUnauthorizedChallenge = true)
        {
            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return await httpResponseMessage.Content.ReadAsStreamAsync();
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
                            return await HandleStreamResponse(authenticatedResponse, request, false);
                        }

                        await HandleNonSuccess(authenticatedResponse);
                        return null;
                    }
                }
            }

            await HandleNonSuccess(httpResponseMessage);

            return null;
        }

        async Task HandleNonSuccess(HttpResponseMessage responseMessage)
        {
            if (responseMessage.StatusCode == HttpStatusCode.RequestTimeout ||
                responseMessage.StatusCode == HttpStatusCode.GatewayTimeout)
            {
                var exception = new HttpRequestException(string.Format("{0}\r\n{1}: Request timeout", responseMessage.RequestMessage.RequestUri, responseMessage.StatusCode));
                ShareFileClient.Logging.Error(exception, "", null);
            }

            if (responseMessage.Content.Headers.ContentLength == 0)
            {
                var exception = new NullReferenceException("Unable to retrieve HttpResponseMessage.Content");
                ShareFileClient.Logging.Error(exception, "", null);
                throw exception;
            }

            if (responseMessage.StatusCode == HttpStatusCode.Unauthorized)
            {
                var supportedSchemes = responseMessage.Headers.WwwAuthenticate.Select(x => x.Scheme).ToList();

                var exception = new WebAuthenticationException("Authentication failed with status code: " + (int)responseMessage.StatusCode, supportedSchemes);
                exception.RequestUri = responseMessage.RequestMessage.RequestUri;
                ShareFileClient.Logging.Error(exception, "", null);
                throw exception;
            }

            if (responseMessage.StatusCode == HttpStatusCode.ProxyAuthenticationRequired)
            {
                var exception = new ProxyAuthenticationException("ProxyAuthentication failed with status code: " + (int)responseMessage.StatusCode);
                ShareFileClient.Logging.Error(exception, "", null);
                throw exception;
            }

            using (var responseStream = await responseMessage.Content.ReadAsStreamAsync())
            {
                if (responseStream != null)
                {
                    var exception = new StreamReader(responseStream).ReadToEnd();
                    Exception exceptionToThrow;
                    try
                    {
                        var requestException = JsonConvert.DeserializeObject<ODataRequestException>(exception);
                        if (requestException.Error != null)
                        {
                            ShareFileClient.Logging.Error(requestException.Error, "", null);
                            exceptionToThrow = requestException.Error;
                        }
                        else
                        {
                            var odataException = JsonConvert.DeserializeObject<ODataException>(exception);
                            ShareFileClient.Logging.Error(odataException, "", null);
                            exceptionToThrow = odataException;
                        }
                    }
                    catch (Exception ex)
                    {
                        var invalidResponseException = new InvalidApiResponseException(responseMessage.StatusCode, exception, ex);
                        ShareFileClient.Logging.Error(invalidResponseException, "", null);
                        exceptionToThrow = invalidResponseException;
                    }

                    if (exceptionToThrow != null)
                    {
                        throw exceptionToThrow;
                    }
                }
            }
        }

        protected Task<HttpResponseMessage> ExecuteRequestAsync(HttpRequestMessage requestMessage, CancellationToken? token = null)
        {
            if (token == null)
            {
                return HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
            }
            
            return HttpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead, token.Value);
        }
    }
}
