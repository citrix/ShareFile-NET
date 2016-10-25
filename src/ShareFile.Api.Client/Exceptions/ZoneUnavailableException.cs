using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareFile.Api.Client.Exceptions
{
    public class ZoneUnavailableException : Exception
    {
        public Uri RequestUri { get; set; }

        public ZoneUnavailableException(Uri requestUri, string message = null, Exception innerException = null) : base(message, innerException)
        {
            this.RequestUri = requestUri;
        }
    }
}
