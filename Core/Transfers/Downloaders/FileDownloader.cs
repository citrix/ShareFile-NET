using System.Collections.Generic;
using System.IO;
using ShareFile.Api.Models;

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
            var totalBytesToDownload = Item.FileSizeBytes.GetValueOrDefault();
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
