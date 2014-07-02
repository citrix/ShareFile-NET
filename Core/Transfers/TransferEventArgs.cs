using System;

namespace ShareFile.Api.Client.Transfers
{
    public class TransferEventArgs : EventArgs
    {
        public TransferProgress Progress { get; set; }
    }
}