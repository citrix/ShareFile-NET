using System;
using System.Net;
using System.Net.Http;

using ShareFile.Api.Client.Exceptions;

namespace ShareFile.Api.Client.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static bool HasContent(this HttpResponseMessage message)
        {
            return message.Content != null && message.Content.Headers != null
            && message.Content.Headers.ContentLength > 0;
        }

        /// <summary>
        /// Gets the redirection if available.
        /// If an HTTP redirection is returned, then it throws.
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        /// <exception cref="HttpsExpectedException" />
        public static Uri GetSecureRedirect(this HttpResponseMessage message)
        {
            switch (message.StatusCode)
            {
                case HttpStatusCode.Moved:
                case HttpStatusCode.Redirect:
                    Uri redirectTo = message.Headers.Location;
                    if (!redirectTo.IsAbsoluteUri)
                    {
                        redirectTo = new Uri(message.RequestMessage.RequestUri, redirectTo);
                    }
                    if (!redirectTo.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase))
                    {
                        if (string.Equals(redirectTo.Host, "maintenance.sharefile.com", StringComparison.OrdinalIgnoreCase))
                        {
                            throw new ApiDownException();
                        }

                        throw new HttpsExpectedException { RedirectUri = redirectTo };
                    }
                    return redirectTo;
            }
            return null;
        }
    }
}
