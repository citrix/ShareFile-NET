using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Requests.Providers
{
    public interface IAsyncRequestProvider : IRequestProvider
    {
        [NotNull]
        Task ExecuteAsync(IQuery query, CancellationToken token = default(CancellationToken));
        [NotNull]
        Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken token = default(CancellationToken)) where T : class;
        [NotNull]
        Task<T> ExecuteAsync<T>(IFormQuery<T> query, CancellationToken token = default(CancellationToken)) where T : class;
        [NotNull]
        Task<Stream> ExecuteAsync(IStreamQuery query, CancellationToken token = default(CancellationToken));
    }
}
