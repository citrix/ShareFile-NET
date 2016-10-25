using System.Net.Http;

namespace ShareFile.Api.Client.Events
{
    public delegate EventHandlerResponse ExceptionCallback(HttpResponseMessage responseMessage, int retryCount);
}
