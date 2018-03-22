using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public abstract class AsyncDownloaderBase : DownloaderBase
    {
        protected AsyncDownloaderBase(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base(item, client, config)
        { }

        protected AsyncDownloaderBase(DownloadSpecification downloadSpecification, IShareFileClient client, DownloaderConfig config = null)
            : base(downloadSpecification, client, config)
        { }

        /// <summary>
        /// Prepares the downloader instance.
        /// </summary>
        public async Task PrepareDownloadAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            DownloadSpecification = await CreateDownloadSpecificationAsync(cancellationToken);
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
        public virtual async Task DownloadToAsync(
            Stream stream,
            CancellationToken cancellationToken = default(CancellationToken),
            Dictionary<string, object> transferMetadata = null,
            RangeRequest rangeRequest = null)
        {
            if (rangeRequest != null && DownloadSpecification == null)
            {
                throw new InvalidOperationException("Downloader instance has not been prepared. In order to supply a RangeRequest, you must first call PrepareDownloadAsync.");
            }

            if (DownloadSpecification == null)
            {
                await PrepareDownloadAsync(cancellationToken);
            }
            else if (Item != null && !await SupportsDownloadSpecificationAsync(Item.GetObjectUri()))
            {
                throw new NotSupportedException("Provider does not support download with DownloadSpecification)");
            }

            rangeRequest = rangeRequest ?? Config.RangeRequest;

            CancellationTokenSource downloadCancellationSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            try
            {
                progressReporter.StartReporting(transferMetadata, downloadCancellationSource.Token);
                await InternalDownloadAsync(stream, rangeRequest, cancellationToken);
                progressReporter.ReportCompletion();
            }
            finally
            {
                downloadCancellationSource.Cancel();
                downloadCancellationSource.Dispose();
            }
        }

        protected abstract Task InternalDownloadAsync(Stream stream, RangeRequest rangeRequest, CancellationToken cancellationToken);
    }
}