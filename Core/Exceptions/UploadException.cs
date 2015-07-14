using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ShareFile.Api.Client.Exceptions
{
    public class UploadException : Exception
    {
        public UploadException(string errorMessage, int errorCode, Exception baseException = null) 
            : base(string.Format("ErrorCode: {0}" + Environment.NewLine + "Message: {1}", errorCode, errorMessage), baseException)
        {

        }
    }
}
