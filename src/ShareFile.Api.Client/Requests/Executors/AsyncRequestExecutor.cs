using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Requests.Executors
{
    public class AsyncRequestExecutor : IAsyncRequestExecutor
    {
        public async Task<HttpResponseMessage> SendAsync(HttpClient httpClient, HttpRequestMessage requestMessage, HttpCompletionOption httpCompletionOption,
            CancellationToken cancellationToken)
        {
            return await httpClient.SendAsync(requestMessage, httpCompletionOption, cancellationToken).ConfigureAwait(false);
        }
    }
}