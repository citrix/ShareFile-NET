using System.Collections.Generic;
using System.IO;
using ShareFile.Api.Client.Models;
using System.Threading;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public abstract class SyncDownloaderBase : DownloaderBase
    {
        protected SyncDownloaderBase(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base(item, client, config)
        {
        }

        protected SyncDownloaderBase(DownloadSpecification downloadSpecification, IShareFileClient client, DownloaderConfig config = null)
            : base(downloadSpecification, client, config)
        {
        }

        /// <summary>
        /// Prepares the downloader instance.
        /// </summary>
        public void PrepareDownload(CancellationToken cancellationToken = default(CancellationToken))
        {
            DownloadSpecification = CreateDownloadSpecification();
        }

        /// <summary>
        /// Downloads the file to the provided Stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="transferMetadata"></param>
        /// <param name="cancellationToken"></param>
        /// <param name="rangeRequest">
        ///     Overrides Config.RangeRequest. ShareFile may have some restrictions on the number of times a range request can be issued for a given download session.  
        ///     There is a chance that providing this can result in failures.
        /// </param>
        public abstract void DownloadTo(Stream stream,
            Dictionary<string, object> transferMetadata = null,
            CancellationToken cancellationToken = default(CancellationToken),
            RangeRequest rangeRequest = null);
    }
}
