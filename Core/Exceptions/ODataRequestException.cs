using System;
using System.Net;
using Newtonsoft.Json;

namespace ShareFile.Api.Client.Exceptions
{
    internal class ODataRequestExceptionAlt
    {
        [JsonProperty("code")]
        public HttpStatusCode Code { get; set; }
        [JsonProperty("message")]
        public ODataExceptionMessage Message { get; set; }
    }

    internal class ODataRequestException
    {
        public ODataException Error { get; set; }
    }

    public class ODataException : Exception
    {
        public HttpStatusCode Code { get; set; }
        public ODataExceptionMessage ODataExceptionMessage { get; set; }
    }

    public class ODataExceptionMessage
    {
        [JsonProperty("lang")]
        public string Language { get; set; }
        [JsonProperty("value")]
        public string Message { get; set; }
    }
}
