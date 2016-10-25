using System;
using System.Net;

using ShareFile.Api.Client.Enums;
using ShareFile.Api.Client.Transfers;

namespace ShareFile.Api.Client.Exceptions
{
    public class UploadException : Exception
    {
        public readonly ActiveUploadState ActiveUploadState;

        public readonly UploadStatusCode StatusCode;

        public HttpStatusCode? HttpStatusCode { get; set; }

        public UploadException(string errorMessage, UploadStatusCode errorCode, Exception baseException = null)
            : base(string.Format("ErrorCode: {0}" + Environment.NewLine + "Message: {1}", errorCode, errorMessage), baseException)
        {
            StatusCode = errorCode;
        }

        public UploadException(string errorMessage, UploadStatusCode errorCode, ActiveUploadState activeUploadState, Exception baseException = null)
            : base(string.Format("ErrorCode: {0}" + Environment.NewLine + "Message: {1}", errorCode, errorMessage), baseException)
        {
            StatusCode = errorCode;
            ActiveUploadState = activeUploadState;
        }

        // 4.1.2016 - The string check is for older versions of SZC that do not return the error code
        public bool IsInvalidUploadId
        {
            get
            {
                const string BadUploadIdMessage = "Unrecognized Upload ID";
                return StatusCode == UploadStatusCode.InvalidUploadId || Message.Contains(BadUploadIdMessage)
                       || (InnerException != null && InnerException.Message.Contains(BadUploadIdMessage));
            }  
        } 
    }
}