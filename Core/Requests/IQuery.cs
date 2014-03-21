using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Requests.Filters;

namespace ShareFile.Api.Client.Requests
{
    public interface IQuery
    {
        void Execute();
        Task ExecuteAsync(CancellationToken? token = null);

        void AddHeader(string key, string value);
    }

    public interface IQuery<T>
        where T : class
    {
        T Execute();
        Task<T> ExecuteAsync(CancellationToken? token = null);

        Query<T> Filter(IFilter filter);
        Query<T> Expand(string expandProperty);
        Query<T> Expand(IEnumerable<string> expandProperties);
        Query<T> Select(string selectProperty);
        Query<T> Select(IEnumerable<string> selectProperties);

        Query<T> OrderBy(string orderByProperty);
        Query<T> Skip(int skip);
        Query<T> Top(int top);

        Query<T> AddHeader(string key, string value);
    }

    public interface IReadOnlyQuery
    {
        string HttpMethod { get; }
        object Body { get; }
        string GetEntity();
        IEnumerable<ODataAction> GetSubActions();
        ODataAction GetAction();
        IDictionary<string, string> GetHeaders();
        ODataParameterCollection GetQueryString();
        ODataParameterCollection GetIds();
    }

    public interface IReadOnlyODataQuery : IReadOnlyQuery
    {
        int GetTop();
        int GetSkip();
        IEnumerable<string> GetSelectProperties();
        IEnumerable<string> GetExpandProperties();
        IFilter GetFilter();
    }
}
