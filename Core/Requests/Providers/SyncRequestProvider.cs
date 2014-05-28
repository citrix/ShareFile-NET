using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ShareFile.Api.Client.Credentials;
using ShareFile.Api.Client.Events;
using ShareFile.Api.Client.Exceptions;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Logging;
using ShareFile.Api.Client.Security.Authentication.OAuth2;
using ShareFile.Api.Client.Security.Cryptography;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Requests.Providers
{
    internal class SyncRequestProvider : BaseRequestProvider, ISyncRequestProvider
    {
        public SyncRequestProvider(ShareFileClient client) : base(client) { }

        public void Execute(IQuery query)
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(IQuery<T> query) where T : class
        {
            throw new NotImplementedException();
        }

        public T Execute<T>(IFormQuery<T> query) where T : class
        {
            throw new NotImplementedException();
        }

        public Stream Execute(IStreamQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
