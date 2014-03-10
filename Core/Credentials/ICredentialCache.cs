using System;
using System.Net;

namespace ShareFile.Api.Client
{
    public interface ICredentialCache : ICredentials
    {
        void Add(Uri uri, string authType, ICredentials credentials);
        void Remove(Uri uri, string authType);
    }
}