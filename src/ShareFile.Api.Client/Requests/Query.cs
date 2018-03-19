using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests.Filters;
using ShareFile.Api.Client.Requests.Providers;
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Requests
{
    public abstract class QueryBase : IReadOnlyQuery
    {
// ReSharper disable InconsistentNaming
        protected string _entity;
        protected readonly ODataParameterCollection _ids;
        protected ODataAction _action;
        protected IList<ODataAction> _subActions;
        protected readonly ODataParameterCollection _queryString;
        protected IDictionary<string, string> _headerCollection;
        protected Uri _baseUri;
        protected QueryMetadata _metadata;
// ReSharper restore InconsistentNaming

        protected QueryBase(IShareFileClient client)
        {
            Client = client;

            _queryString = new ODataParameterCollection();
            _ids = new ODataParameterCollection();
            _subActions = new List<ODataAction>();
            _headerCollection = new Dictionary<string, string>();

            HttpMethod = "GET";
        }

        public IShareFileClient Client { get; internal set; }

        public QueryMetadata Metadata => _metadata;
        public string HttpMethod { get; set; }
        public object Body { get; set; }

        protected void _Id(string id)
        {
            _Ids(null, id);
        }

        protected void _Ids(string id)
        {
            _Ids(null, id);
        }

        protected void _Ids(string key, string id)
        {
            var parameter = new ODataParameter(key, id);

            _ids.AddOrUpdate(parameter);
        }

        protected void _From(string fromEntity)
        {
            _entity = fromEntity;
        }

        protected void _Action(string action)
        {
            if (_action == null)
            {
                _action = new ODataAction();
            }

            _action.ActionName = action;
        }

        protected void _ActionIds(string id)
        {
            _ActionIds(null, id);
        }

        protected void _ActionIds(string key, string id)
        {
            if (_action == null)
            {
                _action = new ODataAction();
            }
            
            var parameter = new ODataParameter(key, id);

            _action.Parameters.AddOrUpdate(parameter);
        }

        protected void _SubAction(string subAction)
        {
            _SubAction(subAction, null, null);
        }

        protected void _SubAction(string subAction, string id)
        {
            _SubAction(subAction, null, id);
        }

        protected void _SubAction(string subAction, string key, string id)
        {
            var action =
                _subActions.FirstOrDefault(x => x.ActionName.Equals(subAction, StringComparison.OrdinalIgnoreCase));

            if (action == null)
            {
                action = new ODataAction
                {
                    ActionName = subAction
                };
                _subActions.Add(action);
            }

            if (!string.IsNullOrEmpty(id))
            {
                var parameter = new ODataParameter(key, id);

                action.Parameters.AddOrUpdate(parameter);
            }
        }

        protected void _QueryString(string key, string value)
        {
            var parameter = new ODataParameter(key, value);

            _queryString.AddOrUpdate(parameter);
        }

        protected void _QueryString(string key, object value)
        {
            if (value is DateTime)
            {
                _QueryString(key, ((DateTime)value).ToString("O"));
            }
            else
            {
                _QueryString(key, Convert.ToString(value));
            }
        }

        protected void _AddHeader(string key, string value)
        {
            _headerCollection[key] = value;
        }

        public ODataParameterCollection GetIds()
        {
            return _ids;
        }

        public ODataParameterCollection GetQueryString()
        {
            return _queryString;
        }

        public IDictionary<string, string> GetHeaders()
        {
            return _headerCollection;
        }

        public ODataAction GetAction()
        {
            return _action ?? new ODataAction();
        }

        public IEnumerable<ODataAction> GetSubActions()
        {
            return _subActions;
        }

        public string GetEntity()
        {
            return _entity;
        }

        public Uri GetBaseUri()
        {
            return _baseUri;
        }

        protected void SetBaseUri(Uri uri)
        {
            string baseUri;

            if (!TryGetUriRoot(uri, out baseUri))
            {
                throw new ArgumentException("Unable to create a BaseUri from the provided uri.  The uri start the following format: https://secure.sf-api.com/sf/v3/");
            }

            _baseUri = new Uri(baseUri, UriKind.Absolute);
        }

        protected bool TryGetUriRoot(Uri providedUri, out string uriRoot)
        {
            uriRoot = null;
            var pathComponents = providedUri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (pathComponents.Length > 1)
            {
                uriRoot = string.Format("{0}://{1}/{2}/{3}/", providedUri.Scheme, providedUri.Host, pathComponents[0], pathComponents[1]);
                return true;
            }
            return false;
        }
    }

    public class Query : QueryBase, IQuery
    {
        public Query(IShareFileClient client)
            : base (client)
        {
            
        }
        
        public Query Id(object id)
        {
            return Id(id.ToString());
        }

        public Query Id(string id)
        {
            _Id(id);
            return this;
        }

        public Query Uri(Uri uri)
        {
            _Ids(uri.ToString());
            return this;
        }

        public Query Ids(object id)
        {
            return Ids(id.ToString());
        }

        public Query Ids(string id)
        {
            _Ids(id);
            return this;
        }

        public Query Ids(string key, object id)
        {
            return Ids(key, id.ToString());
        }

        public Query Ids(string key, string id)
        {
            _Ids(key, id);
            return this;
        }

        public Query From(string fromEntity)
        {
            _From(fromEntity);
            return this;
        }

        public Query Action(string action)
        {
            _Action(action);
            return this;
        }

        public Query ActionIds(object id)
        {
            return ActionIds(id.ToString());
        }

        public Query ActionIds(string id)
        {
            _ActionIds(id);
            return this;
        }

        public Query ActionIds(string key, object id)
        {
            return ActionIds(key, id.ToString());
        }

        public Query ActionIds(string key, string id)
        {
            _ActionIds(key, id);
            return this;
        }

        public Query SubAction(string subAction)
        {
            _SubAction(subAction);
            return this;
        }

        public Query SubAction(string subAction, object id)
        {
            return SubAction(subAction, id.ToString());
        }

        public Query SubAction(string subAction, string id)
        {
            _SubAction(subAction, id);
            return this;
        }

        public Query SubAction(string subAction, string key, object id)
        {
            return SubAction(subAction, key, id.ToString());
        }

        public Query SubAction(string subAction, string key, string id)
        {
            _SubAction(subAction, key, id);
            return this;
        }

        public Query QueryString(string key, string value)
        {
            _QueryString(key, value);
            return this;
        }

        public Query QueryString(string key, object value)
        {
            _QueryString(key, value);
            return this;
        }

        public void Execute()
        {
            Client.Execute(this);
        }

        [NotNull]
        public Task ExecuteAsync(CancellationToken token = default(CancellationToken))
        {
            return Client.ExecuteAsync(this, token);
        }

        public Query AddHeader(string key, string value)
        {
            _AddHeader(key, value);
            return this;
        }

        public Query WithBaseUri(Uri uri)
        {
            SetBaseUri(uri);
            return this;
        }

        public IQuery WithMetadata(QueryMetadata metadata)
        {
            _metadata = metadata;
            return this;
        }
    }

    public class Query<T> : QueryBase, IQuery<T>, IReadOnlyODataQuery
        where T : class
    {

// ReSharper disable InconsistentNaming
        protected readonly IList<string> _selectProperties;
        protected readonly IList<string> _expandProperties;
        protected IFilter _filterCriteria;
        protected int _skip;
        protected int _top;
        protected string _orderBy;
// ReSharper restore InconsistentNaming

        public Query(IShareFileClient client)
            : base (client)
        {
            _selectProperties = new List<string>();
            _expandProperties = new List<string>();
            _skip = 0;
            _top = -1;
        }

        internal static Query<TTo> Copy<TFrom, TTo>(Query<TFrom> copyFrom, Query<TTo> copyTo)
            where TFrom : class
            where TTo : class
        {
            copyTo._action = copyFrom._action;
            copyTo._baseUri = copyFrom._baseUri;
            copyTo._entity = copyFrom._entity;
            copyTo._filterCriteria = copyFrom._filterCriteria;
            copyTo._headerCollection = copyFrom._headerCollection;
            copyTo._orderBy = copyFrom._orderBy;
            copyTo._skip = copyFrom._skip;
            copyTo._subActions = copyFrom._subActions;
            copyTo._top = copyFrom._top;
            copyTo.Body = copyFrom.Body;
            copyTo.HttpMethod = copyFrom.HttpMethod;
            foreach (var expand in copyFrom._expandProperties) copyTo._expandProperties.Add(expand);
            foreach (var id in copyFrom._ids) copyTo._ids.Add(id);
            foreach (var qsParam in copyFrom._queryString) copyTo._queryString.Add(qsParam);
            foreach (var select in copyFrom._selectProperties) copyTo._selectProperties.Add(select);
            return copyTo;
        }

        public IQuery<U> Expect<U>()
            where U : class
        {
            Query<U> query = new Query<U>(Client);
            return Query<U>.Copy(this, query);
        }

        public Query<T> Id(object id)
        {
            return Id(id.ToString());
        }

        public Query<T> Id(string id)
        {
            _Id(id);
            return this;
        }

        public Query<T> Uri(Uri uri)
        {
            _Ids(uri.ToString());
            return this;
        }

        public Query<T> Ids(object id)
        {
            return Ids(id.ToString());
        }

        public Query<T> Ids(string id)
        {
            _Ids(id);
            return this;
        }

        public Query<T> Ids(string key, object id)
        {
            return Ids(key, id.ToString());
        }

        public Query<T> Ids(string key, string id)
        {
            _Ids(key, id);
            return this;
        }

        public Query<T> From(string fromEntity)
        {
            _From(fromEntity);
            return this;
        }

        public Query<T> Action(string action)
        {
            _Action(action);
            return this;
        }

        public Query<T> ActionIds(object id)
        {
            return ActionIds(id.ToString());
        }

        public Query<T> ActionIds(string id)
        {
            _ActionIds(id);
            return this;
        }

        public Query<T> ActionIds(string key, object id)
        {
            return ActionIds(key, id.ToString());
        }

        public Query<T> ActionIds(string key, string id)
        {
            _ActionIds(key, id);
            return this;
        }

        public Query<T> SubAction(string subAction)
        {
            _SubAction(subAction);
            return this;
        }

        public Query<T> SubAction(string subAction, object id)
        {
            return SubAction(subAction, id.ToString());
        }

        public Query<T> SubAction(string subAction, string id)
        {
            _SubAction(subAction, id);
            return this;
        }

        public Query<T> SubAction(string subAction, string key, object id)
        {
            return SubAction(subAction, key, id.ToString());
        }

        public Query<T> SubAction(string subAction, string key, string id)
        {
            _SubAction(subAction, key, id);
            return this;
        }

        public Query<T> QueryString(string key, string value)
        {
            _QueryString(key, value);
            return this;
        }

        public Query<T> QueryString(string key, object value)
        {
            _QueryString(key, value);
            return this;
        }

        /// <summary>
        /// If a Filter has already been added, it will implicitly converted to a <see cref="AndFilter"/> 
        /// with the existing filter as Left and <param name="filter"></param> as Right.
        /// </summary>
        /// <param name="filter"></param>
        /// <returns></returns>
        public Query<T> Filter(IFilter filter)
        {
            if (_filterCriteria != null)
            {
                _filterCriteria = new AndFilter(_filterCriteria, filter);
            }
            else
            {
                _filterCriteria = filter;
            }

            return this;
        }

        private static char[] CommaCharArray = { ',' };
        public Query<T> Expand(string expandProperty)
        {
            return Expand(expandProperty.Split(CommaCharArray, StringSplitOptions.RemoveEmptyEntries));
        }

        public Query<T> Expand(IEnumerable<string> expandProperties)
        {
            foreach (var expandProperty in expandProperties)
            {
                _expandProperties.Add(expandProperty);
            }

            return this;
        }

        public Query<T> Select(string selectProperty)
        {
            return Select(selectProperty.Split(CommaCharArray, StringSplitOptions.RemoveEmptyEntries));
        }

        public Query<T> Select(IEnumerable<string> selectProperties)
        {
            foreach (var selectProperty in selectProperties)
            {
                _selectProperties.Add(selectProperty);
            }

            return this;
        }

        public Query<T> OrderBy(string orderByProperty)
        {
            _orderBy = orderByProperty;
            return this;
        }

        public Query<T> OrderBy(string orderByProperty, SortDirection direction)
        {
            switch (direction)
            {
                case SortDirection.Ascending:
                    _orderBy = orderByProperty + " " + "asc";
                    break;
                case SortDirection.Descending:
                    _orderBy = orderByProperty + " " + "desc";
                    break;
                default:
                    _orderBy = orderByProperty;
                    break;
            }
            return this;
        }

        public Query<T> Skip(int skip)
        {
            _skip = skip;
            return this;
        }

        public Query<T> Top(int top)
        {
            _top = top;
            return this;
        }

        public Query<T> AddHeader(string key, string value)
        {
            _AddHeader(key, value);
            return this;
        }

        public Query<T> WithBaseUri(Uri uri)
        {
            SetBaseUri(uri);
            return this;
        }

        public virtual T Execute()
        {
            if (this is IQuery<Stream>)
            {
                return Client.Execute((IQuery<Stream>)this) as T;
            }
            return Client.Execute(this);
        }

		[NotNull]
        public virtual Task<T> ExecuteAsync(CancellationToken token = default(CancellationToken))
        {
            if (this is IQuery<Stream>)
            {
                return (Task<T>)(object)Client.ExecuteAsync((IQuery<Stream>)this, token);
            }
            return Client.ExecuteAsync(this, token);
        }

        public int GetTop()
        {
            return _top;
        }

        public int GetSkip()
        {
            return _skip;
        }

        public IEnumerable<string> GetSelectProperties()
        {
            return _selectProperties.OrderBy(select => select.Length).Distinct();
        }

        public IEnumerable<string> GetExpandProperties()
        {
            return _expandProperties.OrderBy(expand => expand.Length).Distinct(); ;
        }

        public IFilter GetFilter()
        {
            return _filterCriteria;
        }

        public string GetOrderBy()
        {
            return _orderBy;
        }

        public IQuery<T> WithMetadata(QueryMetadata metadata)
        {
            _metadata = metadata;
            return this;
        }
    }

    public interface IFormQuery<T> : IQuery<T>
        where T : class
    {
        
    }

    public class FormQuery<T> : Query<T>, IFormQuery<T>
        where T : class
    {
        public FormQuery(IShareFileClient client) : base(client)
        {
        }
    }

    public interface IStreamQuery : IQuery<Stream>
    {
        
    }

    public class StreamQuery : Query<Stream>, IStreamQuery
    {
        public StreamQuery(IShareFileClient client) : base(client)
        {

        }

        public override Stream Execute()
        {
            return Client.Execute(this);
        }
		
        [NotNull]
        public override Task<Stream> ExecuteAsync(CancellationToken token = default(CancellationToken))
        {
            return Client.ExecuteAsync(this, token);
        }
    }

    internal class MappedQuery<SourceType, TargetType> : Query<TargetType>
        where SourceType : class
        where TargetType : class
    {
        private Func<SourceType, TargetType> map;

        public MappedQuery(Query<SourceType> query, Func<SourceType, TargetType> map) : base(query.Client)
        {
            Query<SourceType>.Copy(query, this);
            this.map = map;
        }

        public override TargetType Execute()
        {
            Query<SourceType> query = Query<TargetType>.Copy(this, new Query<SourceType>(this.Client));
            SourceType result = query.Execute();
            return map(result);
        }
		
        [NotNull]
        public override async Task<TargetType> ExecuteAsync(CancellationToken token = default(CancellationToken))
        {
            Query<SourceType> query = Query<TargetType>.Copy(this, new Query<SourceType>(this.Client));
            SourceType result = await query.ExecuteAsync(token).ConfigureAwait(false);
            var targetResult = map(result);
            return Constraint.NotNull(() => targetResult);
        }
    }

    public class ApiRequest
    {
        public Uri Uri { get; set; }
        public IDictionary<string, string> HeaderCollection { get; set; }
        public string HttpMethod { get; set; }
        public object Body { get; set; }
        public ODataParameterCollection QueryStringCollection { get; set; }
        public QueryBase QueryBase { get; protected set; }
        /// <summary>
        /// Indicates whether or not the Uri has been composed.
        /// </summary>
        public bool IsComposed { get; set; }

        public ApiRequest()
        {
            QueryStringCollection = new ODataParameterCollection();
            HeaderCollection = new Dictionary<string, string>();
        }

        /// <summary>
        /// Check if the provided id is a fully qualified <see cref="Uri"/>
        /// </summary>
        /// <param name="id"></param>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool IsUri(string id, out Uri uri)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                uri = null;
                return false;
            }
            return Uri.TryCreate(id, UriKind.Absolute, out uri);
        }

        public static ApiRequest FromQuery(QueryBase query, string requestId = null)
        {
            var ids = query.GetIds().ToString();
            var action = query.GetAction().ActionName;
            var actionParameters = query.GetAction().Parameters;
            var queryString = query.GetQueryString();
            var entityType = query.GetEntity();
            var client = query.Client;
            var subActions = query.GetSubActions().ToList();
            var queryBaseUri = query.GetBaseUri();
            
            StringBuilder url;
            Uri idsUri;
            if (IsUri(ids, out idsUri))
            {
                if (string.IsNullOrWhiteSpace(idsUri.Query))
                {
                    url = new StringBuilder(ids);
                }
                else
                {
                    foreach (var odataParameter in idsUri.GetQueryAsODataParameters())
                    {
                        queryString.Add(odataParameter);
                    }

                    url = new StringBuilder(ids.Substring(0, ids.IndexOf('?')));
                }
            }
            else
            {
                var baseUri = queryBaseUri ?? client.BaseUri;
                if (baseUri == null) throw new ArgumentNullException("client.BaseUri", "baseUri cannot be null, ensure the property has been passed in correctly");
                url = new StringBuilder(baseUri.ToString().TrimEnd('/') + '/' + entityType);
                if (!string.IsNullOrEmpty(ids))
                {
                    url.AppendFormat("({0})", query.GetIds().ToStringForUri());
                }
            }

            if (!string.IsNullOrEmpty(action))
            {
                url.AppendFormat("/{0}", action);

                if (actionParameters.Count > 0)
                {
                    url.Append("(");
                    url.Append(actionParameters.ToStringForUri());
                    url.Append(")");
                }
            }

            if (subActions.Any())
            {
                foreach (var subAction in subActions)
                {
                    url.AppendFormat("/{0}", subAction.ActionName);
                    if (subAction.Parameters.Count > 0)
                    {
                        url.Append("(");
                        url.Append(subAction.Parameters.ToStringForUri());
                        url.Append(")");
                    }
                }
            }

            var apiRequest = new ApiRequest
            {
                Uri = new Uri(url.ToString()),
                QueryBase = query,
                HttpMethod = query.HttpMethod,
                Body = query.Body
            };

            var readOnlyODataQuery = query as IReadOnlyODataQuery;
            if (readOnlyODataQuery != null)
            {
                var skip = readOnlyODataQuery.GetSkip();
                var top = readOnlyODataQuery.GetTop();
                var filters = readOnlyODataQuery.GetFilter();
                var select = string.Join(",", readOnlyODataQuery.GetSelectProperties());
                var expand = string.Join(",", readOnlyODataQuery.GetExpandProperties());
                var orderBy = readOnlyODataQuery.GetOrderBy();

                if (!string.IsNullOrEmpty(select))
                {
                    apiRequest.QueryStringCollection.Add(new ODataParameter("$select", select));
                }
                if (!string.IsNullOrEmpty(expand))
                {
                    apiRequest.QueryStringCollection.Add(new ODataParameter("$expand", expand));
                }
                if (top > 0)
                {
                    apiRequest.QueryStringCollection.Add(new ODataParameter("$top",
                        top.ToString(CultureInfo.InvariantCulture)));
                }
                if (skip > 0)
                {
                    apiRequest.QueryStringCollection.Add(new ODataParameter("$skip",
                        skip.ToString(CultureInfo.InvariantCulture)));
                }
                if (!string.IsNullOrEmpty(orderBy))
                {
                    apiRequest.QueryStringCollection.Add(new ODataParameter("$orderby", orderBy));
                }
                
                if (filters != null)
                {
                    apiRequest.QueryStringCollection.Add(new ODataParameter("$filter", filters.ToString()));
                }
            }

            if(queryString != null)
            {
                foreach (var kvp in queryString)
                {
                    apiRequest.QueryStringCollection.Add(new ODataParameter(kvp.Key, kvp.Value));
                }
            }

            apiRequest.HeaderCollection = query.GetHeaders();

            return apiRequest;
        }

        protected string GetQueryStringForUri()
        {
            if (QueryStringCollection.Count > 0)
            {
                var sb = new StringBuilder();
                foreach (var kvp in QueryStringCollection)
                {
                    sb.AppendFormat("{0}={1}&", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value ?? string.Empty));
                }
                return sb.ToString().TrimEnd('&');
            }
            return null;
        }

        public Uri GetComposedUri()
        {
            if (IsComposed) return Uri;
            var queryString = GetQueryStringForUri();
            var bridgeChar = (Uri.ToString().IndexOf('?') < 0) ? '?' : '&';
            return string.IsNullOrEmpty(queryString)
                ? Uri
                : new Uri(string.Format("{0}{1}{2}", Uri, bridgeChar, queryString));
        }
    }
}
