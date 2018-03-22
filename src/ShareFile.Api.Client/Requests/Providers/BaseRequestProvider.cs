using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ShareFile.Api.Client.Converters;
using ShareFile.Api.Client.Credentials;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Requests.Providers
{
    public abstract class BaseRequestProvider
    {
        internal const int MaxAutomaticRedirections = 50;

        internal static class Headers
        {
            public const string HttpOverrideHeader = "X-Http-Method-Override";
            public const string ClientCapabilities = "X-SF-ClientCapabilities";
        }

        public ShareFileClient ShareFileClient { get; protected set; }

        /// <summary>
        /// Set this flag True if running as a Portable Class Library
        /// </summary>
#if PORTABLE || NETSTANDARD1_3
        public static bool RuntimeRequiresCustomCookieHandling = true;
#else
        public static bool RuntimeRequiresCustomCookieHandling = false;
#endif

        public HttpClient HttpClient
        {
            get { return ShareFileClient.HttpClient; }
        }
        private readonly string requestId;

        public string RequestId
        {
            get { return requestId; }
        }

        protected BaseRequestProvider(ShareFileClient shareFileClient)
        {
            requestId = GenerateRequestId();
            ShareFileClient = shareFileClient;
        }

        protected HttpRequestMessage BuildRequest(ApiRequest request)
        {
            HttpRequestMessage requestMessage;
            var watch = new ActionStopwatch("BuildRequest", ShareFileClient.Logging, RequestId);
            var uri = request.GetComposedUri();

            if (ShareFileClient.Configuration.UseHttpMethodOverride)
            {
                requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);
                requestMessage.Headers.Add(Headers.HttpOverrideHeader, request.HttpMethod);
            }
            else
            {
                if (request.HttpMethod == "GET" && request.Body != null)
                {
                    request.HttpMethod = "POST";
                }

                requestMessage = new HttpRequestMessage(new HttpMethod(request.HttpMethod), uri);
            }

            requestMessage.TryAddAuthorizationHeaders(ShareFileClient, GetAuthorizationHeaderValue);
            requestMessage.AddDefaultHeaders(ShareFileClient);

            LogRequestUri(request);

            foreach (var kvp in request.HeaderCollection)
            {
                requestMessage.Headers.Add(kvp.Key, kvp.Value);
            }

            requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if (request.Body != null)
            {
                try
                {
                    WriteRequestBody(requestMessage, request.Body, new MediaTypeHeaderValue("application/json"));
                    LogRequest(request, requestMessage.Headers.ToString());
                }
                catch (Exception)
                {
                    requestMessage.Dispose();
                    throw;
                }
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

        protected HttpCompletionOption GetCompletionOptionForQuery(Type queryTypeParameter)
        {
            if (queryTypeParameter.Equals(typeof(Stream)))
                return HttpCompletionOption.ResponseHeadersRead;
            else
                return HttpCompletionOption.ResponseContentRead;
        }

        internal static void TryAddCookies(ShareFileClient client, HttpRequestMessage requestMessage)
        {
            if (RuntimeRequiresCustomCookieHandling)
            {
                var cookieHeader = client.CookieContainer.GetCookieHeader(
                    new Uri("https://www." + requestMessage.RequestUri.Host + requestMessage.RequestUri.AbsolutePath));

                if (!string.IsNullOrWhiteSpace(cookieHeader))
                {
                    requestMessage.Headers.Add("Cookie", cookieHeader);
                }
                else
                {
                    cookieHeader = client.CookieContainer.GetCookieHeader(requestMessage.RequestUri);
                    if (!string.IsNullOrEmpty(cookieHeader))
                    {
                        requestMessage.Headers.Add("Cookie", cookieHeader);
                    }
                }
            }
        }

        protected T DeserializeResponseStream<T>(Stream responseStream, HttpResponseMessage httpResponseMessage)
        {
            var watch = new ActionStopwatch("DeserializeStream", ShareFileClient.Logging, RequestId);

            TextReader responseReader;
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                string response = "";
                using (var responseStreamReader = new StreamReader(responseStream))
                {
                    response = responseStreamReader.ReadToEnd();
                }
                ShareFileClient.Logging.Debug(response);
                responseReader = new StringReader(response);
            }
            else
            {
                responseReader = new StreamReader(responseStream);
            }

            try
            {
                using (var reader = new JsonTextReader(responseReader))
                {
                    return ShareFileClient.Serializer.Deserialize<T>(reader);
                }
            }
            catch(JsonException jEx)
            {
                string contentType = httpResponseMessage.Content != null && httpResponseMessage.Content.Headers != null && httpResponseMessage.Content.Headers.ContentType != null
                    ? httpResponseMessage.Content.Headers.ContentType.ToString() 
                    : "(no content-type)";
                throw new InvalidApiResponseException(httpResponseMessage.StatusCode,
                    String.Format("Unexpected (non-JSON) response format {0}", contentType),
                    jEx);
            }
            finally
            {
                ShareFileClient.Logging.Trace(watch);
                if(responseReader != null)
                {
                    responseReader.Dispose();
                }
            }
        }
		
        protected Task<T> DeserializeResponseStreamAsync<T>(Stream responseStream, HttpResponseMessage httpResponseMessage)
        {
            return Task.Factory.StartNew(() => DeserializeResponseStream<T>(responseStream, httpResponseMessage));
        }

        protected Task<string> SerializeObjectAsync(object obj)
        {
            var tcs = new TaskCompletionSource<string>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    tcs.SetResult(SerializeObject(obj));
                }
                catch (Exception)
                {
                    tcs.SetResult(null);
                }
            }).ConfigureAwait(false);

            return tcs.Task;
        }

        protected async Task LogRequestAsync(ApiRequest request, string headers)
        {
            LogRequestUri(request);

            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                if (ShareFileClient.Configuration.LogCookiesAndHeaders)
                {
                    ShareFileClient.Logging.Debug("[{0}] Headers: {1}", new object[] { RequestId, headers });
                    LogCookies(request.GetComposedUri());
                }
                if (request.Body != null)
                {
                    ShareFileClient.Logging.Debug("[{0}] Content:{1}{2}",
                        new object[]
                        {
                            RequestId, Environment.NewLine,
                            await SerializeObjectAsync(request.Body).ConfigureAwait(false)
                        });
                }
            }
        }

        protected async Task LogResponseAsync<T>(T response, Uri requestUri, string headers, HttpStatusCode statusCode, MediaTypeHeaderValue mediaType = null)
        {
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                ShareFileClient.Logging.Debug("[{0}] Response Code: {1}", new object[] { RequestId, statusCode });
                if (ShareFileClient.Configuration.LogCookiesAndHeaders)
                {
                    ShareFileClient.Logging.Debug("[{0}] {1}", new object[] { RequestId, headers });
                    LogCookies(requestUri);
                }
                if (response != null)
                {
                    if (mediaType == null || mediaType.MediaType == "application/json")
                    {
                        ShareFileClient.Logging.Debug("[{0}] Content:{1}{2}", new object[] { RequestId, Environment.NewLine, await SerializeObjectAsync(response).ConfigureAwait(false) });
                    }
                    else ShareFileClient.Logging.Debug("[{0}] Content {1}", new object[] { RequestId, response.ToString() });
                }
            }
            else if (ShareFileClient.Logging.IsTraceEnabled)
            {
                ShareFileClient.Logging.Trace("[{0}] Response Code: {1}", new object[] { RequestId, statusCode });
            }
        }

        protected void LogRequest(ApiRequest request, string headers)
        {
            LogRequestUri(request);

            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                if (ShareFileClient.Configuration.LogCookiesAndHeaders)
                {
                    ShareFileClient.Logging.Debug("[{0}] Headers: {1}", new object[] { RequestId, headers });
                    LogCookies(request.GetComposedUri());
                }
                if (request.Body != null)
                {
                    ShareFileClient.Logging.Debug("[{0}] Content:{1}{2}", new object[] { RequestId, Environment.NewLine, SerializeObject(request.Body) });
                }
            }
        }

        protected void LogResponse<T>(T response, Uri requestUri, string headers, HttpStatusCode statusCode, MediaTypeHeaderValue mediaType = null)
        {
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                ShareFileClient.Logging.Debug("[{0}] Response Code: {1}", new object[] {RequestId, statusCode});
                if (ShareFileClient.Configuration.LogCookiesAndHeaders)
                {
                    ShareFileClient.Logging.Debug("[{0}] {1}", new object[] { RequestId, headers });
                    LogCookies(requestUri);
                }
                if (response != null)
                {
                    if (mediaType == null || mediaType.MediaType == "application/json")
                    {
                        ShareFileClient.Logging.Debug("[{0}] Content:{1}{2}", new object[] { RequestId, Environment.NewLine, SerializeObject(response) });
                    }
                    else ShareFileClient.Logging.Debug("[{0}] Content {1}", new object[] { RequestId, response.ToString() });
                }
            }
            else if (ShareFileClient.Logging.IsTraceEnabled)
            {
                ShareFileClient.Logging.Trace("[{0}] Response Code: {1}", new object[] { RequestId, statusCode });
            }
        }

        protected string SerializeObject(object obj)
        {
            if (obj is HttpResponseMessage)
            {
                return obj.ToString();
            }
            try
            {
                var stringWriter = new StringWriter();
                using (var textWriter = new JsonTextWriter(stringWriter))
                {
                    ShareFileClient.LoggingSerializer.Serialize(textWriter, obj);

                    return stringWriter.ToString();
                }

            }
            catch (Exception)
            {
                return null;
            }
        }

        protected void LogRequestUri(ApiRequest request)
        {
            var requestUri = request.GetComposedUri().ToString();

            if (!ShareFileClient.Configuration.LogPersonalInformation)
            {
                LoggingConverter.GuidRegex.Replace(requestUri, LoggingConverter.GetHash);
            }

            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                ShareFileClient.Logging.Debug("[{0}] {1} {2}", new object[] {RequestId, request.HttpMethod, requestUri});
            }
            else if (ShareFileClient.Logging.IsTraceEnabled)
            {
                ShareFileClient.Logging.Trace("[{0}] {1} {2}", new object[] { RequestId, request.HttpMethod, requestUri });
            }
        }

        protected void LogCookies(Uri uri)
        {
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                var cookieCollection = ShareFileClient.CookieContainer.GetCookies(uri);
                var stringBuilder = new StringBuilder(string.Format("[{0}] Cookies: ", RequestId));

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

                    httpRequestMessage.Content = new StringContent(stringWriter.ToString());

                    if (httpRequestMessage.Content.Headers.ContentLength > 0)
                        httpRequestMessage.Content.Headers.ContentType = contentType;
                }
            }
            else if (body is IDictionary<string, string>)
            {
                var formContent = new FormUrlEncodedContent(body as IDictionary<string, string>);

                if (ShareFileClient.Logging.IsDebugEnabled)
                {
                    try
                    {
                        var contentAsString = formContent.ReadAsStringAsync();
                    }
                    catch (Exception)
                    {
                        formContent.Dispose();
                        throw;
                    }
                }

                httpRequestMessage.Content = formContent;
            }
            else
            {
                var stringWriter = new StringWriter();
                using (var textWriter = new JsonTextWriter(stringWriter))
                {
                    var serializationWatch = new ActionStopwatch("SerializeRequest", ShareFileClient.Logging, RequestId);
                    ShareFileClient.Serializer.Serialize(textWriter, body);
                    ShareFileClient.Logging.Trace(serializationWatch);

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

                if (Configuration.IsNetCore)
                {
                    // Hack: Latest System.Net.Http doesn't automatically add this from the credential cache
                    if (networkCredential != System.Net.CredentialCache.DefaultNetworkCredentials
                        && wwwAuthenticateHeader.Scheme == "Basic")
                    {
                        var userName = networkCredential.UserName;
                        if (!string.IsNullOrEmpty(networkCredential.Domain))
                        {
                            userName = networkCredential.Domain + "\\" + userName;
                        }
                        var byteArray =
                            Encoding.UTF8.GetBytes(string.Format("{0}:{1}", userName, networkCredential.Password));
                        return new AuthenticationHeaderValue(
                            wwwAuthenticateHeader.Scheme,
                            Convert.ToBase64String(byteArray));
                    }
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

        protected void ProcessCookiesForRuntime(HttpResponseMessage responseMessage)
        {
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
        }

        protected void CheckAsyncOperationScheduled(object responseObject)
        {
            if (responseObject is ODataFeed<AsyncOperation>)
            {
                throw new AsyncOperationScheduledException(responseObject as ODataFeed<AsyncOperation>);
            }
        }

        public static string GenerateRequestId()
        {
            return Guid.NewGuid().ToString("N").Substring(0, 16);
        }
    }
}
