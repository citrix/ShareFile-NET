using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

using ShareFile.Api.Client.Requests.Providers;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Extensions
{
    public static class HttpRequestMessageExtensions
    {
        public static void AddDefaultHeaders(this HttpRequestMessage requestMessage, ShareFileClient client)
        {
            if (client.Configuration.SupportedCultures != null)
            {
                foreach (var cultureInfo in client.Configuration.SupportedCultures)
                {
                    requestMessage.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(cultureInfo.Name));
                }
            }

            if (client.Configuration.ClientCapabilities != null)
            {
                var provider = ShareFileClient.GetProvider(requestMessage.RequestUri);
                IEnumerable<ClientCapability> clientCapabilities;
                if (client.Configuration.ClientCapabilities.TryGetValue(provider, out clientCapabilities))
                {
                    requestMessage.Headers.Add(BaseRequestProvider.Headers.ClientCapabilities, clientCapabilities.Select(x => x.ToString()));
                }
            }

            if (!string.IsNullOrEmpty(client.Configuration.UserAgent))
            {
                requestMessage.Headers.Add("User-Agent", client.Configuration.UserAgent);
            }

            if (!string.IsNullOrEmpty(client.Configuration.ToolName))
            {
                requestMessage.Headers.Add("X-SFAPI-Tool", client.Configuration.ToolName);
            }

            if(!string.IsNullOrEmpty(client.Configuration.ToolVersion))
            {
                requestMessage.Headers.Add("X-SFAPI-ToolVersion", client.Configuration.ToolVersion);
            }

            requestMessage.TryAddCookies(client);
        }

        public static void TryAddCookies(this HttpRequestMessage requestMessage, ShareFileClient client)
        {
            if (BaseRequestProvider.RuntimeRequiresCustomCookieHandling)
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

        public static void TryAddAuthorizationHeaders(this HttpRequestMessage requestMessage, ShareFileClient client, Func<Uri, string, AuthenticationHeaderValue> getAuthenticationHeader)
        {
            var isLoginRequest = requestMessage.RequestUri.AbsolutePath.Contains("Sessions/Login");
            if (client.Configuration.AlwaysPresentCredentials || isLoginRequest)
            {
                var authorizationHeader = getAuthenticationHeader(requestMessage.RequestUri, "Bearer");
                if (authorizationHeader != null)
                {
                    requestMessage.Headers.Authorization = authorizationHeader;
                }
            }
        }

        public static HttpRequestMessage Clone(this HttpRequestMessage req, Uri newUri = null)
        {
            var clone = new HttpRequestMessage(HttpMethod.Get, newUri ?? req.RequestUri);
            clone.Version = req.Version;
            return clone;
        }
    }
}
