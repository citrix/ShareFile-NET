using System.Collections.Generic;
using System.Threading;

namespace ShareFile.Api.Client.Transfers
{
    public class TransferProgress
    {
        private long bytesTransferred;

        private long bytesRemaining;

        private int complete;

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

        public bool Complete
        {
            get
            {
                return this.complete == 1;
            }
        }

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

        public TransferProgress()
        {
            
        }

        public void IncrementBytesTransferred(long increment)
        {
            Interlocked.Add(ref bytesTransferred, increment);
        }

        public void DecrementBytesRemaining(long increment)
        {
            Interlocked.Add(ref bytesRemaining, increment * -1);
        }

        public void MarkComplete()
        {
            Interlocked.Exchange(ref complete, 1);
        }
    }
}
