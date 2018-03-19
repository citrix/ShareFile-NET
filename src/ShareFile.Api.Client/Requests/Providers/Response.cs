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
using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Requests.Providers
{
    public class Response<T> : Response
    {
        public T Value { get; set; }
    }

    public class Response
    {
        public EventHandlerResponse Action { get; set; }
        public static Response<TSuccess> CreateSuccess<TSuccess>(TSuccess value)
        {
            return new Response<TSuccess>
            {
                Value = value
            };
        }

        public static Response<TSuccess> CreateAction<TSuccess>(EventHandlerResponse response)
        {
            return new Response<TSuccess>
            {
                Action = response
            };
        }

        public static Response CreateAction(EventHandlerResponse response)
        {
            return new Response
            {
                Action = response
            };
        }

        public static Response Success = new Response();
    }
}
