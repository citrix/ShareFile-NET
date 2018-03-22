using System;

namespace ShareFile.Api.Client.Transfers
{
#if Net40
    public class TransferEventArgs : EventArgs

#else
    public struct TransferEventArgs
#endif
    {
        public TransferProgress Progress;
    }
}