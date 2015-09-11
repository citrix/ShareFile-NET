using System.Net.Http;

namespace ShareFile.Api.Client.Extensions
{
    public static class HttpResponseMessageExtensions
    {
        public static bool HasContent(this HttpResponseMessage message)
        {
            return message.Content != null && message.Content.Headers != null
            && message.Content.Headers.ContentLength > 0;
        }
    }
}
