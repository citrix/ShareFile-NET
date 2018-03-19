using System;

using ShareFile.Api.Client.Models;

namespace ShareFile.Api.Client.Transfers
{
    public class ActiveUploadState
    {
        public readonly UploadSpecification UploadSpecification;

        public readonly long BytesUploaded;

        public ActiveUploadState(UploadSpecification uploadSpecification, long bytesUploaded)
        {
            UploadSpecification = uploadSpecification;
            BytesUploaded = bytesUploaded;
        }
    }
}