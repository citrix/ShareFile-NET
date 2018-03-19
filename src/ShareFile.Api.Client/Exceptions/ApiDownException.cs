using System;

namespace ShareFile.Api.Client.Exceptions
{
    public class ApiDownException : Exception
    {
        public ApiDownException()
            : base("The API is currently in maintenance mode. Try again later.")
        {
        }
    }
}
