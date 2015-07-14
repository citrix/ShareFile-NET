﻿using System;
using System.Net;
using Newtonsoft.Json;

namespace ShareFile.Api.Client.Exceptions
{
    public class ODataRequestException
    {
        [JsonProperty("code")]
        public HttpStatusCode Code { get; set; }
        [JsonProperty("message")]
        public ODataExceptionMessage Message { get; set; }
    }

    public class ODataException : Exception
    {
        public HttpStatusCode Code { get; set; }
        public ODataExceptionMessage ODataExceptionMessage { get; set; }

        public override string Message
        {
            get
            {
                if (ODataExceptionMessage == null)
                    return base.Message;
                return ODataExceptionMessage.Message;
            }
        }
    }

    public class ODataExceptionMessage
    {
        [JsonProperty("lang")]
        public string Language { get; set; }
        [JsonProperty("value")]
        public string Message { get; set; }
    }
}
