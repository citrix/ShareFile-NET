using System.Net.Http;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Events
{
    public delegate EventHandlerResponse ChangeDomainCallback(HttpRequestMessage requestMessage, Redirection redirect);
}
