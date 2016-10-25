using System;

namespace ShareFile.Api.Client.Enums
{
    public enum UploadStatusCode
    {
        Ok = 0,
        Unknown = -1,
        Cancelled = -2,
        InvalidUploadId = 605
    }
}