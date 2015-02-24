using System.Collections.Generic;

namespace ShareFile.Api.Client.Transfers
{
    public class TransferProgress
    {
        private long bytesTransferred;

        private long bytesRemaining;

        public long BytesTransferred
	    {
            get
            {
                return bytesTransferred;
            }
	    }

        public long BytesRemaining
        {
            get
            {
                return this.bytesRemaining;
            }
        }

        public bool Complete { get; set; }

        public long TotalBytes { get; private set; }
        public string TransferId { get; private set; }
        public IDictionary<string, object> TransferMetadata { get; internal set; }

        public TransferProgress(long totalBytes, IDictionary<string, object> transferMetadata = null, string transferId = null)
        {
            TotalBytes = totalBytes;
            TransferId = transferId;
            bytesRemaining = totalBytes;
            TransferMetadata = transferMetadata;
        }

        internal TransferProgress UpdateBytesTransferred(long transferred)
        {
            if (bytesTransferred + transferred < 0)
            {
                bytesTransferred = 0;
                bytesRemaining = TotalBytes;
            }
            else
            {
                bytesTransferred += transferred;
                bytesRemaining -= transferred;
            }

            return this;
        }

        public TransferProgress MarkComplete()
        {
            Complete = true;

            return this;
        }
    }
}
