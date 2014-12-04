using System;
using System.Net.Http;

namespace ShareFile.Api.Client.Security
{
    public abstract class CustomAuthentication
    {
        public abstract Uri SignUri(Uri uri);
        public abstract HttpRequestMessage SignBody(object body, HttpRequestMessage requestMessage);

        public virtual bool UsesHmacSha256
        {
            get { return true; } 
        }
    }
}
