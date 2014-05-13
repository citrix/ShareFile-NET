using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Requests.Filters;
using ShareFile.Api.Models;

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
            _QueryString(key, Convert.ToString(value));
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
    }

    public class Query : QueryBase, IQuery
    {
        public Query(IShareFileClient client)
            : base (client)
        {
            
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

        public Query Ids(string id)
        {
            _Ids(id);
            return this;
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

        public Query ActionIds(string id)
        {
            _ActionIds(id);
            return this;
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

        public Query SubAction(string subAction, string id)
        {
            _SubAction(subAction, id);
            return this;
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

        public Task ExecuteAsync(CancellationToken? token = null)
        {
            return Client.ExecuteAsync(this, token);
        }

        public void AddHeader(string key, string value)
        {
            _AddHeader(key, value);
        }
    }

    public class Query<T> : QueryBase, IQuery<T>, IReadOnlyODataQuery
        where T : class
    {

// ReSharper disable InconsistentNaming
        protected readonly IList<string> _selectProperties;
        protected readonly IList<string> _expandProperties;
        protected readonly IList<IFilter> _filterCriteria;
        protected int _skip;
        protected int _top;
        protected string _orderBy;
// ReSharper restore InconsistentNaming

        public Query(IShareFileClient client)
            : base (client)
        {
            _selectProperties = new List<string>();
            _expandProperties = new List<string>();
            _filterCriteria = new List<IFilter>();
            _skip = 0;
            _top = -1;
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

        public Query<T> Ids(string id)
        {
            _Ids(id);
            return this;
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

        public Query<T> ActionIds(string id)
        {
            _ActionIds(id);
            return this;
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

        public Query<T> SubAction(string subAction, string id)
        {
            _SubAction(subAction, id);
            return this;
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

        public Query<T> Filter(IFilter filter)
        {
            _filterCriteria.Clear();
            _filterCriteria.Add(filter);

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

        public virtual T Execute()
        {
            return Client.Execute(this);
        }

        public virtual Task<T> ExecuteAsync(CancellationToken? token = null)
        {
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
            return _selectProperties;
        }

        public IEnumerable<string> GetExpandProperties()
        {
            return _expandProperties;
        }

        public IFilter GetFilter()
        {
            return _filterCriteria.FirstOrDefault();
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

        public override Task<Stream> ExecuteAsync(CancellationToken? token = null)
        {
            return Client.ExecuteAsync(this, token);
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
        public bool IsComposed { get; set; }

        public ApiRequest()
        {
            QueryStringCollection = new ODataParameterCollection();
            HeaderCollection = new Dictionary<string, string>();
        }

        public static bool IsUri(string id)
        {
            return id.StartsWith("http://") || id.StartsWith("https://");
        }

        public static ApiRequest FromQuery(QueryBase query)
        {
            var ids = query.GetIds().ToString();
            var action = query.GetAction().ActionName;
            var actionParameters = query.GetAction().Parameters;
            var queryString = query.GetQueryString();
            var entityType = query.GetEntity();
            var client = query.Client;
            var subActions = query.GetSubActions().ToList();
            
            StringBuilder url;
            if (IsUri(ids))
            {
                url = new StringBuilder(ids);
            }
            else
            {
                var baseUri = client.GetRequestBaseUri();
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
                    foreach (var i in actionParameters)
                    {
                        url.AppendFormat("{0},", actionParameters.ToStringForUri());
                    }
                    url.Remove(url.Length - 1, 1);
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
                        foreach (var parameter in subAction.Parameters)
                        {
                            url.AppendFormat("{0},", parameter.ToStringForUri());
                        }
                        url.Remove(url.Length - 1, 1);
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
            return string.IsNullOrEmpty(queryString)
                ? Uri
                : new Uri(string.Format("{0}?{1}", Uri, queryString));
        }
    }
}
