using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Converters;
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
    public abstract class BaseRequestProvider
    {
        public ShareFileClient ShareFileClient { get; protected set; }

        /// <summary>
        /// Set this flag True if running as a Portable Class Library
        /// </summary>
#if Portable
        public static bool RuntimeRequiresCustomCookieHandling = true;
#else
        public static bool RuntimeRequiresCustomCookieHandling = false;
#endif
        
        public HttpClient HttpClient { get; set; }

        protected BaseRequestProvider(ShareFileClient shareFileClient)
        {
            ShareFileClient = shareFileClient;

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

            if (shareFileClient.Configuration.ProxyConfiguration != null)
            {
                handler.Proxy = shareFileClient.Configuration.ProxyConfiguration;
                handler.UseProxy = true;
            }

            HttpClient = new HttpClient(handler, false)
            {
                Timeout = new TimeSpan(0, 0, 0, 0, shareFileClient.Configuration.HttpTimeout)
            };
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

            if (ShareFileClient.Configuration.SupportedCultures != null)
            {
                foreach (var cultureInfo in ShareFileClient.Configuration.SupportedCultures)
                {
                    requestMessage.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(cultureInfo.Name));
                }
            }

            LogRequestUri(request);

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
                else
                {
                    cookieHeader = ShareFileClient.CookieContainer.GetCookieHeader(requestMessage.RequestUri);
                    if (!string.IsNullOrEmpty(cookieHeader))
                    {
                        requestMessage.Headers.Add("Cookie", cookieHeader);
                    }
                }
            }

            ShareFileClient.Logging.Trace(watch);

            return requestMessage;
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

#if Async
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
                    JsonTextReader reader = new JsonTextReader(responseStreamReader);

                    responseValue = ShareFileClient.Serializer.Deserialize<T>(reader);
                }


                ShareFileClient.Logging.Trace(watch);
                tcs.SetResult(responseValue);

            }).ConfigureAwait(false);

            return task;
        }

        protected Task<string> SerializeObjectAsync(object obj)
        {
            var tcs = new TaskCompletionSource<string>();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    var stringWriter = new StringWriter();
                    using (var textWriter = new JsonTextWriter(stringWriter))
                    {
                        ShareFileClient.LoggingSerializer.Serialize(textWriter, obj);

                        tcs.SetResult(stringWriter.ToString());
                    }

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
                    ShareFileClient.Logging.Debug("Headers: {0}", headers);
                    LogCookies(request.GetComposedUri());
                }
                if (request.Body != null)
                {
                    ShareFileClient.Logging.Debug("Content:{0}{1}", Environment.NewLine, await SerializeObjectAsync(request.Body).ConfigureAwait(false));
                }
            }
        }

        protected async Task LogResponseAsync<T>(T response, Uri requestUri, string headers, HttpStatusCode statusCode, MediaTypeHeaderValue mediaType = null)
        {
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                ShareFileClient.Logging.Debug("Response Code: {0}", statusCode);
                if (ShareFileClient.Configuration.LogCookiesAndHeaders)
                {
                    ShareFileClient.Logging.Debug("{0}", headers);
                    LogCookies(requestUri);
                }
                if (response != null)
                {
                    if (mediaType == null || mediaType.MediaType == "application/json")
                    {
                        ShareFileClient.Logging.Debug("Content:{0}{1}", Environment.NewLine, await SerializeObjectAsync(response).ConfigureAwait(false));
                    }
                    else ShareFileClient.Logging.Debug("Content {0}", response.ToString());
                }
            }
            else if (ShareFileClient.Logging.IsTraceEnabled)
            {
                ShareFileClient.Logging.Trace("Response Code: {0}", statusCode);
            }
        }
#endif

        protected void LogRequest(ApiRequest request, string headers)
        {
            LogRequestUri(request);

            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                if (ShareFileClient.Configuration.LogCookiesAndHeaders)
                {
                    ShareFileClient.Logging.Debug("Headers: {0}", headers);
                    LogCookies(request.GetComposedUri());
                }
                if (request.Body != null)
                {
                    ShareFileClient.Logging.Debug("Content:{0}{1}", Environment.NewLine, SerializeObject(request.Body));
                }
            }
        }

        protected void LogResponse<T>(T response, Uri requestUri, string headers, HttpStatusCode statusCode, MediaTypeHeaderValue mediaType = null)
        {
            if (ShareFileClient.Logging.IsDebugEnabled)
            {
                ShareFileClient.Logging.Debug("Response Code: {0}", statusCode);
                if (ShareFileClient.Configuration.LogCookiesAndHeaders)
                {
                    ShareFileClient.Logging.Debug("{0}", headers);
                    LogCookies(requestUri);
                }
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

        protected string SerializeObject(object obj)
        {
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
                ShareFileClient.Logging.Debug("{0} {1}", request.HttpMethod, requestUri);
            }
            else if (ShareFileClient.Logging.IsTraceEnabled)
            {
                ShareFileClient.Logging.Trace("{0} {1}", request.HttpMethod, requestUri);
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
            else if (body is IDictionary<string, string>)
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
                var stringWriter = new StringWriter();
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
    }
}
