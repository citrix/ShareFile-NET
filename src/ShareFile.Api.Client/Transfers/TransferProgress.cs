using System.Collections.Generic;

namespace ShareFile.Api.Client.Transfers
{
    public struct TransferProgress
    {
        public long BytesTransferred;
        public long BytesRemaining;
        public bool Complete;
        public long TotalBytes;
        public string TransferId;
        public IDictionary<string, object> TransferMetadata;
    }
}
