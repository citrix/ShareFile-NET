#if NETFX_CORE
using System;

namespace ShareFile.Api.Client.Exceptions
{
    public class ApplicationException : Exception
    {
        public ApplicationException(string message, Exception innerException = null) :
            base(message, innerException)
        {
            
        }
    }
}
#endif
