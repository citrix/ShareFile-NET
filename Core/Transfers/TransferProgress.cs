using System.Collections.Generic;

namespace ShareFile.Api.Client.Transfers
{
    public class TransferProgress
    {
        public long BytesTransferred { get; set; }

        public long BytesRemaining { get; set; }

        public bool Complete { get; set; }

        public long TotalBytes { get; set; }
        public string TransferId { get; set; }
        public IDictionary<string, object> TransferMetadata { get; internal set; }

        public TransferProgress(long totalBytes, IDictionary<string, object> transferMetadata = null, string transferId = null)
        {
            TotalBytes = totalBytes;
            TransferId = transferId;
            BytesRemaining = totalBytes;
            TransferMetadata = transferMetadata;
        }

        public TransferProgress()
        {
            
        }

        internal TransferProgress UpdateBytesTransferred(long transferred)
        {
            if (BytesTransferred + transferred < 0)
            {
                BytesTransferred = 0;
                BytesRemaining = TotalBytes;
            }
            else
            {
                BytesTransferred += transferred;
                BytesRemaining -= transferred;
            }

            return this;
        }

        internal TransferProgress MarkComplete()
        {
            Complete = true;

            return this;
        }
    }
}
