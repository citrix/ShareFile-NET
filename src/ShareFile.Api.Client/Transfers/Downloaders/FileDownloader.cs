using System.Collections.Generic;
using System.IO;
using ShareFile.Api.Models;
using System.Threading;
using ShareFile.Api.Client.Extensions.Tasks;
using System;
using ShareFile.Api.Client.Extensions;

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

        public override void DownloadTo(Stream fileStream,
            Dictionary<string, object> transferMetadata = null,
            CancellationToken? cancellationToken = null,
            RangeRequest rangeRequest = null)
        {
            if (rangeRequest != null && DownloadSpecification == null)
            {
                throw new InvalidOperationException("Downloader instance has not been prepared. In order to supply a RangeRequest instance, you must first call PrepareDownload.");
            }

            if (DownloadSpecification == null)
            {
                PrepareDownload();
            }
            else if (!SupportsDownloadSpecification(Item.GetObjectUri()))
            {
                throw new NotSupportedException("Provider does not support download with DownloadSpecification)");
            }

            var downloadQuery = CreateDownloadStreamQuery(rangeRequest);

            var totalBytesToDownload = rangeRequest != null
                ? rangeRequest.End.GetValueOrDefault() - rangeRequest.Begin.GetValueOrDefault()
                : Item.FileSizeBytes.GetValueOrDefault();

            var progress = new TransferProgress(totalBytesToDownload, transferMetadata);

            using (var stream = downloadQuery.Execute())
            {
                if (stream != null)
                {
                    int bytesRead;
                    var buffer = new byte[Config.BufferSize];

                    do
                    {
                        TryPause();
                        cancellationToken.ThrowIfRequested();

                        bytesRead = stream.Read(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);

                            NotifyProgress(progress.UpdateBytesTransferred(bytesRead));
                        }

                    } while (bytesRead > 0);
                }
            }

            NotifyProgress(progress.MarkComplete());
        }
    }
}
