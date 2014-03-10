namespace ShareFile.Api.Client.Requests.Providers
{
    public interface ISyncRequestProvider : IRequestProvider
    {
        void Execute(IQuery query);
        T Execute<T>(IQuery<T> query);
    }

    public interface IRequestProvider
    {
        ShareFileClient Client { get; set; }
    }
}
