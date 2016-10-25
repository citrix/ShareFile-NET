using System;

namespace ShareFile.Api.Client.Exceptions
{
    public class HttpsExpectedException : Exception
    {
        public HttpsExpectedException()
            : base("A redirect request will change a secure to a non-secure connection")
        {
        }
    }
}
