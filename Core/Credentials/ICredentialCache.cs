using System;
using System.Net;

namespace ShareFile.Api.Client.Credentials
{
    public interface ICredentialCache : ICredentials
    {
        void Add(Uri uri, string authType, NetworkCredential credentials);
        void Remove(Uri uri, string authType);
    }
}