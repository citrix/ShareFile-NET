using System.Collections.Generic;
using System.IO;
using ShareFile.Api.Client.Models;
using System.Threading;
using ShareFile.Api.Client.Extensions.Tasks;
using System;
using ShareFile.Api.Client.Extensions;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public class FileDownloader : SyncDownloaderBase
    {
        public FileDownloader(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base(item, client, config)
        {
        }

        public FileDownloader(DownloadSpecification downloadSpecification, IShareFileClient client, DownloaderConfig config = null)
            : base(downloadSpecification, client, config)
        {
        }

        public override void DownloadTo(Stream outputStream,
            Dictionary<string, object> transferMetadata = null,
            CancellationToken cancellationToken = default(CancellationToken),
            RangeRequest rangeRequest = null)
        {
            if (rangeRequest != null && DownloadSpecification == null)
            {
                throw new InvalidOperationException("Downloader instance has not been prepared. In order to supply a RangeRequest instance, you must first call PrepareDownload.");
            }

            if (DownloadSpecification == null)
            {
                PrepareDownload(cancellationToken);
            }
            else if (Item != null && !SupportsDownloadSpecification(Item.GetObjectUri()))
            {
                throw new NotSupportedException("Provider does not support download with DownloadSpecification)");
            }

            rangeRequest = rangeRequest ?? Config.RangeRequest;

            var downloadQuery = CreateDownloadStreamQuery(rangeRequest);
            CancellationTokenSource downloadCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            Stream downloadStream = null;
            try
            {
                progressReporter.StartReporting(transferMetadata, downloadCancellationSource.Token);
                downloadStream = downloadQuery.Execute();
                downloadStream = ExpectedLengthStream(downloadStream);

                int bytesRead;
                var buffer = new byte[Configuration.BufferSize];
                do
                {
                    TryPause();
                    if (downloadCancellationSource.Token.IsCancellationRequested)
                        throw new TaskCanceledException();

                    bytesRead = downloadStream.Read(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        outputStream.Write(buffer, 0, bytesRead);
                        progressReporter.ReportProgress(bytesRead);
                    }

                } while (bytesRead > 0);
            }
            finally
            {
                downloadCancellationSource.Cancel();
                downloadCancellationSource.Dispose();
                downloadStream?.Dispose();
            }
            progressReporter.ReportCompletion();
        }
    }
}
