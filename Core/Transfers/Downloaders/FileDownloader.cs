using System;
using System.Collections.Generic;
using System.IO;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Models;
using ShareFile.Api.Client.Extensions;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public class FileDownloader : SyncDownloaderBase
    {
        public FileDownloader(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base(item, client, config)
        {

        }

        public override void DownloadTo(Stream fileStream, Dictionary<string, object> transferMetadata = null)
        {
            var downloadQuery = CreateDownloadQuery();

            using (var stream = downloadQuery.Execute())
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
                        bytesRead = stream.Read(buffer, 0, buffer.Length);
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
    }

    public abstract class SyncDownloaderBase : DownloaderBase
    {
        protected SyncDownloaderBase(Item item, IShareFileClient client, DownloaderConfig config = null) 
            : base(item, client, config)
        {
        }

        public abstract void DownloadTo(Stream fileStream, Dictionary<string, object> transferMetadata = null);
    }

    public abstract class DownloaderBase : TransfererBase
    {
        internal Item Item { get; set; }
        internal IShareFileClient Client { get; set; }
        public DownloaderConfig Config { get; set; }

        public EventHandler<TransferEventArgs> OnTransferProgress;

        protected DownloaderBase(Item item, IShareFileClient client, DownloaderConfig config = null)
        {
            Client = client;
            Item = item;
            Config = config ?? new DownloaderConfig();
        }

        protected void NotifyProgress(TransferProgress progress)
        {
            if (OnTransferProgress != null)
            {
                OnTransferProgress.Invoke(this, new TransferEventArgs { Progress = progress });
            }
        }

        protected StreamQuery CreateDownloadQuery()
        {
            var downloadQuery = new StreamQuery(Client).Uri(Item.GetObjectUri()).Action("Download");

            if (Config != null && Config.RangeRequest != null)
            {
                if (!Config.RangeRequest.End.HasValue)
                {
                    downloadQuery.AddHeader("Range", string.Format("bytes={0}", Config.RangeRequest.Begin.GetValueOrDefault()));
                }
                else
                {
                    downloadQuery.AddHeader("Range",
                        string.Format("bytes={0}-{1}", Config.RangeRequest.Begin.GetValueOrDefault(),
                            Config.RangeRequest.End.GetValueOrDefault()));
                }
            }

            return downloadQuery as StreamQuery;
        }
    }
}
