using ShareFile.Api.Client.Extensions.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers
{
    public class TransferProgressReporter
    {
        public event EventHandler<TransferEventArgs> OnTransferProgress;

        private readonly TimeSpan reportInterval;
        private readonly long fileSize;
        private readonly string transferId;

        private long progress = 0;
        private IDictionary<string, object> transferMetadata;

        public TransferProgressReporter(long fileSize, string transferId, TimeSpan reportInterval)
        {
            this.fileSize = fileSize;
            this.reportInterval = reportInterval;
            this.transferId = transferId;
        }
        
        public void StartReporting(IDictionary<string, object> transferMetadata, CancellationToken cancellationToken)
        {
            this.transferMetadata = transferMetadata ?? new Dictionary<string, object>(0);
            ReportLoopAsync(cancellationToken).Forget();
        }
        
        private async Task ReportLoopAsync(CancellationToken cancellationToken)
        {
            long lastProgress = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                long currentProgress = Interlocked.Read(ref progress);
                long difference = currentProgress - lastProgress;
                if (difference != 0)
                {
                    OnTransferProgress?.Invoke(this, Args(currentProgress));
                    lastProgress = currentProgress;
                }
                await Task.Delay(reportInterval);
            }
        }

        private TransferEventArgs Args(long currentProgress)
        {
            var progress = new TransferProgress
            {
                TotalBytes = fileSize,
                BytesTransferred = currentProgress,
                BytesRemaining = fileSize - currentProgress,                 
                Complete = false,
                TransferId = transferId,
                TransferMetadata = transferMetadata,
            };
            return new TransferEventArgs { Progress = progress };
        }

        public void ReportProgress(long bytesTransferred) => Interlocked.Add(ref progress, bytesTransferred);

        public void ResetProgress() => Interlocked.Exchange(ref progress, 0);

        public void ReportCompletion()
        {
            var args = Args(fileSize);
            args.Progress.Complete = true;
            OnTransferProgress?.Invoke(this, args);
        }

        public ChunkProgressReporter ChunkProgressReporter() => new ChunkProgressReporter(this);

        internal void ImmediatelyReportProgress(long bytesTransferred)
        {
            ReportProgress(bytesTransferred);
            OnTransferProgress?.Invoke(this, Args(fileSize));
        }
    }   
    
    public class ChunkProgressReporter
    {
        private TransferProgressReporter fileProgressReporter;

        private long progress = 0;

        public ChunkProgressReporter(TransferProgressReporter fileProgressReporter)
        {
            this.fileProgressReporter = fileProgressReporter;
        }

        public void ReportProgress(long bytesTransferred)
        {
            Interlocked.Add(ref progress, bytesTransferred);
            fileProgressReporter.ReportProgress(bytesTransferred);
        }

        public void ResetProgress()
        {
            long progressToRollback = Interlocked.Exchange(ref progress, 0);
            fileProgressReporter.ReportProgress(-1 * progressToRollback);
        }
    }
}