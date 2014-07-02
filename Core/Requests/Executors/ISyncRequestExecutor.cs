using System.Net.Http;

namespace ShareFile.Api.Client.Requests.Executors
{
    public interface ISyncRequestExecutor
    {
        HttpResponseMessage Send(HttpClient client, HttpRequestMessage requestMessage, HttpCompletionOption httpCompletionOption);
    }
}
