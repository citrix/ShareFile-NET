using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public class Downloader
    {
        private IQuery<DownloadSpecification> Query { get; set; }
        public DownloaderConfig Config { get; set; }
        public EventHandler<TransferEventArgs> OnTransferProgress;
        private IShareFileClient Client { get; set; }
        private Item Item { get; set; }

        internal Downloader(Item item, IShareFileClient client, DownloaderConfig config = null)
        {
            Client = client;
            Config = config ?? DownloaderConfig.Default;
            Item = item;
        }

        protected virtual async Task<DownloadSpecification> PrepareDownloadAsync(CancellationToken? cancellationToken)
        {
            var downloadSpecificationQuery = Client.Items.Download(Item.GetObjectUri().ToString(), false);

            return await downloadSpecificationQuery.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        }

        public async Task DownloadToAsync(Stream fileStream, CancellationToken? cancellationToken = null,
            Dictionary<string, object> transferMetadata = null)
        {
            var downloadSpecification = await PrepareDownloadAsync(cancellationToken);

            if (Config != null && Config.RangeRequest != null)
            {
                if (!Config.RangeRequest.End.HasValue)
                {
                    Query.AddHeader("Range", string.Format("bytes={0}", Config.RangeRequest.Begin.GetValueOrDefault()));
                }
                else
                {
                    Query.AddHeader("Range",
                        string.Format("bytes={0}-{1}", Config.RangeRequest.Begin.GetValueOrDefault(),
                            Config.RangeRequest.End.GetValueOrDefault()));
                }
            }

            var streamQuery = new StreamQuery(Client).Ids(downloadSpecification.DownloadUrl.ToString());
            
            using (var stream = await streamQuery.ExecuteAsync(cancellationToken))
            {
                if (stream != null)
                {
                    var totalBytesToDownload = Item.FileSizeBytes.GetValueOrDefault();

                    var progress = new TransferProgress
                    {
                        BytesTransferred = 0,
                        BytesRemaining = totalBytesToDownload,
                        TotalBytes = totalBytesToDownload,
                        TransferMetadata = transferMetadata
                    };

                    int bytesRead;
                    var buffer = new byte[Config.BufferSize];

                    do
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                        if (bytesRead > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);

                            progress.BytesTransferred += bytesRead;
                            progress.BytesRemaining -= bytesRead;

                            NotifyProgress(progress);
                        }
                        else
                        {
                            progress.Complete = true;
                            NotifyProgress(progress);
                        }

                    } while (bytesRead > 0);
                }
            }
        }

        protected void NotifyProgress(TransferProgress progress)
        {
            if (OnTransferProgress != null)
            {
                OnTransferProgress.Invoke(this, new TransferEventArgs { Progress = progress });
            }
        }
    }
}
