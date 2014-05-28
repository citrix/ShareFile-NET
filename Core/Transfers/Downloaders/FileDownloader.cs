using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public class FileDownloader : DownloaderBase
    {
        public FileDownloader(Item item, ShareFileClient client, DownloaderConfig config = null)
            : base(item, client, config)
        {

        }

        protected DownloadSpecification PrepareDownload()
        {
            var downloadSpecificationQuery = Client.Items.Download(Item.url, false);

            return downloadSpecificationQuery.Execute();
        }

        public override void DownloadTo(Stream fileStream, Dictionary<string, object> transferMetadata = null)
        {
            var downloadSpecification = PrepareDownload();

            var streamQuery = new StreamQuery(Client).Ids(downloadSpecification.DownloadUrl.ToString());

            using (var stream = streamQuery.Execute())
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

    public abstract class DownloaderBase
    {
        internal Item Item { get; set; }
        internal ShareFileClient Client { get; set; }
        public DownloaderConfig Config { get; set; }

        public EventHandler<TransferEventArgs> OnTransferProgress;

        protected DownloaderBase(Item item, ShareFileClient client, DownloaderConfig config = null)
        {
            Client = client;
            Item = item;
            Config = config ?? new DownloaderConfig();
        }

        public abstract void DownloadTo(Stream fileStream, Dictionary<string, object> transferMetadata = null);

        protected void NotifyProgress(TransferProgress progress)
        {
            if (OnTransferProgress != null)
            {
                OnTransferProgress.Invoke(this, new TransferEventArgs { Progress = progress });
            }
        }
    }
}
