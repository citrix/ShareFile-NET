using System;
using System.Net;

namespace ShareFile.Api.Client.Exceptions
{
    public class InvalidApiResponseException : Exception
    {
        public HttpStatusCode StatusCode { get; set; }

        public InvalidApiResponseException(HttpStatusCode code, string message, Exception innerException = null)
            : base (message, innerException)
        {
            StatusCode = code;
        }

        public override string ToString()
        {
            return string.Format("StatusCode: {0}{1}{2}", StatusCode, Environment.NewLine, base.ToString());
        }
    }
}
