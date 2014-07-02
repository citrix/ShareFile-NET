using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
#if Async
    public class AsyncFileDownloader : DownloaderBase
    {
        internal AsyncFileDownloader(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base (item, client, config)
        {
            Client = client;
            Config = config ?? DownloaderConfig.Default;
            Item = item;
        }

        public async Task DownloadToAsync(Stream fileStream, CancellationToken? cancellationToken = null,
            Dictionary<string, object> transferMetadata = null)
        {
            var downloadQuery = CreateDownloadQuery();

            using (var stream = await downloadQuery.ExecuteAsync(cancellationToken))
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

                            await TryPause(cancellationToken);
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
#endif
}
