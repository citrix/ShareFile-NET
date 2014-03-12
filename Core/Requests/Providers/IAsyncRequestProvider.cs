using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Requests.Providers
{
    public interface IAsyncRequestProvider : IRequestProvider
    {
        Task ExecuteAsync(IQuery query, CancellationToken? token = null);
        Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken? token = null);
        Task<Stream> ExecuteAsync(IStreamQuery query, CancellationToken? token = null);
    }
}
