using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Requests.Filters;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Requests
{
    public interface IQuery
    {
        QueryMetadata Metadata { get; }

        void Execute();

        [NotNull]
        Task ExecuteAsync(CancellationToken token = default(CancellationToken));

        Query AddHeader(string key, string value);
        Query WithBaseUri(Uri uri);
        IQuery WithMetadata(QueryMetadata metadata);
    }

    public interface IQuery<T>
        where T : class
    {
        QueryMetadata Metadata { get; }

        T Execute();

        [NotNull]
        Task<T> ExecuteAsync(CancellationToken token = default(CancellationToken));
        
        /// <summary>
        /// If a Filter has already been added, it will implicitly converted to a <see cref="AndFilter"/> 
        /// with the existing filter as Left and <param name="filter"></param> as Right.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        Query<T> Filter(IFilter filter);
        Query<T> Expand(string expandProperty);
        Query<T> Expand(IEnumerable<string> expandProperties);
        Query<T> Select(string selectProperty);
        Query<T> Select(IEnumerable<string> selectProperties);

        Query<T> OrderBy(string orderByProperty);

        Query<T> OrderBy(string orderByProperty, SortDirection direction);
        Query<T> Skip(int skip);
        Query<T> Top(int top);

        Query<T> AddHeader(string key, string value);
        Query<T> WithBaseUri(Uri uri);
        IQuery<T> WithMetadata(QueryMetadata metadata);
        IQuery<U> Expect<U>() where U : class;
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
        string GetOrderBy();
    }
}
