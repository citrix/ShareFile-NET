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
        public AsyncFileDownloader(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base (item, client, config)
        {
            Client = client;
            Config = config ?? DownloaderConfig.Default;
            Item = item;
        }

        public async virtual Task DownloadToAsync(Stream fileStream, CancellationToken? cancellationToken = null,
            Dictionary<string, object> transferMetadata = null)
        {
            var downloadQuery = CreateDownloadQuery();
            var totalBytesToDownload = Item.FileSizeBytes.GetValueOrDefault();
            var progress = new TransferProgress(totalBytesToDownload, transferMetadata);

            using (var stream = await downloadQuery.ExecuteAsync(cancellationToken))
            {
                if (stream != null)
                {
                    int bytesRead;
                    var buffer = new byte[Config.BufferSize];

                    do
                    {
                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken ?? CancellationToken.None);
                        if (bytesRead > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);

                            NotifyProgress(progress.UpdateBytesTransferred(bytesRead));

                            await TryPauseAsync(cancellationToken);
                        }

                    } while (bytesRead > 0);
                }
            }

            NotifyProgress(progress.MarkComplete());
        }
    }
#endif
}
