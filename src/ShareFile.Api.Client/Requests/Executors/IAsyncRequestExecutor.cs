using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Requests.Executors
{
    public interface IAsyncRequestExecutor
    {
        Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpRequestMessage requestMessage, HttpCompletionOption httpCompletionOption, CancellationToken cancellationToken);
    }
}
