using System.Net;
using Newtonsoft.Json;

namespace ShareFile.Api.Client.Exceptions
{
    internal class ODataRequestException
    {
        public ODataException Error { get; set; }
    }

    public class ODataException
    {
        [JsonProperty("code")]
        public HttpStatusCode Code { get; set; }
        [JsonProperty("message")]
        public ODataExceptionMessage Message { get; set; }
        //public ODataException(string code, string message, string lang = "en-US")
        //{
        //    HttpStatusCode aCode;
        //    var parsed = EnumHelpers.TryParse(code, out aCode);
        //    if (parsed)
        //    {
        //        Code = aCode;
        //    }
        //    else
        //    {
        //        Code = HttpStatusCode.InternalServerError;
        //    }
            
        //    Message = new ODataExceptionMessage { value = message, lang = lang };
        //}
    }

    public class ODataExceptionMessage
    {
        [JsonProperty("lang")]
        public string Language { get; set; }
        [JsonProperty("value")]
        public string Message { get; set; }
    }
}
