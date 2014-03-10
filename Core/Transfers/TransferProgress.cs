using System.Collections.Generic;

namespace ShareFile.Api.Client.Transfers
{
    public class TransferProgress
    {
        public long BytesTransferred { get; set; }
        public long BytesRemaining { get; set; }
        public long TotalBytes { get; set; }
        public string TransferId { get; set; }
        public bool Complete { get; set; }

        public Dictionary<string, object> TransferMetadata { get; set; }
    }
}
