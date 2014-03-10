using System;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Requests.Providers
{
    public class DefaultRequestProvider : IAsyncRequestProvider, ISyncRequestProvider
    {
        public Task ExecuteAsync(IQuery query, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public Task<T> ExecuteAsync<T>(IQuery<T> query, CancellationToken? token = null)
        {
            throw new NotImplementedException();
        }

        public void Execute(IQuery query)
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(IQuery<T> query)
        {
            throw new NotImplementedException();
        }

        public ShareFileClient Client { get; set; }
    }
}
