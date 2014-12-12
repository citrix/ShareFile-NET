using System.Net.Http;
using ShareFile.Api.Client.Extensions.Tasks;

namespace ShareFile.Api.Client.Requests.Executors
{
    public class SyncRequestExecutor : ISyncRequestExecutor
    {
        public HttpResponseMessage Send(HttpClient httpClient, HttpRequestMessage requestMessage, HttpCompletionOption httpCompletionOption)
        {
            return httpClient.SendAsync(requestMessage, httpCompletionOption).WaitForTask();
        }
    }
}
