using System;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Models;
using System.Threading.Tasks;
using System.Threading;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public abstract class DownloaderBase : TransfererBase
    {
        internal Item Item { get; set; }
        internal IShareFileClient Client { get; set; }
        public DownloaderConfig Config { get; set; }
        protected DownloadSpecification DownloadSpecification { get; set; }

        public EventHandler<TransferEventArgs> OnTransferProgress;

        protected DownloaderBase(Item item, IShareFileClient client, DownloaderConfig config = null)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            Client = client;
            Item = item;
            Config = config ?? new DownloaderConfig();
        }

        protected DownloaderBase(DownloadSpecification downloadSpecification, IShareFileClient client, DownloaderConfig config = null)
        {
            if (downloadSpecification == null)
            {
                throw new ArgumentNullException(nameof(downloadSpecification));
            }

            Client = client;
            Config = config ?? new DownloaderConfig();
            DownloadSpecification = downloadSpecification;
        }

        protected void NotifyProgress(TransferProgress progress)
        {
            if (OnTransferProgress != null)
            {
                OnTransferProgress.Invoke(this, new TransferEventArgs { Progress = progress });
            }
        }

        protected StreamQuery CreateDownloadQuery(RangeRequest rangeRequest)
        {
            return CreateDownloadStreamQuery(Config.RangeRequest);
        }

#if ASYNC
        protected async Task<DownloadSpecification> CreateDownloadSpecificationAsync(CancellationToken token = default(CancellationToken))
        {
            if (Item == null)
            {
                throw new InvalidOperationException("A download specification can not be created.  No item has been specified.");
            }

            if (!await SupportsDownloadSpecificationAsync(Item.GetObjectUri(), token))
            {
                return null;
            }

            return await Client.Items.Download(Item.GetObjectUri(), false).Expect<DownloadSpecification>()
                 .ExecuteAsync(token).ConfigureAwait(false);
        }

        protected async Task<bool> SupportsDownloadSpecificationAsync(Uri objectUri, CancellationToken token = default(CancellationToken))
        {
            var capabilities = Client.GetCachedCapabilities(objectUri)
                ?? (await Client.Capabilities.Get().WithBaseUri(objectUri).ExecuteAsync(token).ConfigureAwait(false)).Feed;

            Client.SetCachedCapabilities(objectUri, capabilities);

            return capabilities.SupportsDownloadWithSpecificaton();
        }
#endif

        protected DownloadSpecification CreateDownloadSpecification()
        {
            if (Item == null)
            {
                throw new InvalidOperationException("A download specification can not be created.  No item has been specified.");
            }

            if (!SupportsDownloadSpecification(Item.GetObjectUri()))
            {
                return null;
            }

            return Client.Items.Download(Item.GetObjectUri(), false).Expect<DownloadSpecification>().Execute();
        }

        protected bool SupportsDownloadSpecification(Uri objectUri)
        {
            var capabilities = Client.GetCachedCapabilities(objectUri)
                ?? Client.Capabilities.Get().WithBaseUri(objectUri).Execute().Feed;

            Client.SetCachedCapabilities(objectUri, capabilities);

            return capabilities.SupportsDownloadWithSpecificaton();
        }

        protected StreamQuery CreateDownloadStreamQuery(RangeRequest rangeRequest)
        {
            rangeRequest = rangeRequest ?? Config?.RangeRequest;

            var downloadQuery = DownloadSpecification == null
                ? new StreamQuery(Client).Uri(Item.GetObjectUri()).Action("Download")
                : new StreamQuery(Client).Uri(DownloadSpecification.DownloadUrl);

            downloadQuery.AddHeader("Accept", "*/*");

            if (rangeRequest != null)
            {
                if (!rangeRequest.End.HasValue)
                {
                    downloadQuery.AddHeader("Range", $"bytes={rangeRequest.Begin.GetValueOrDefault()}-");
                }
                else
                {
                    downloadQuery.AddHeader("Range",
                        $"bytes={rangeRequest.Begin.GetValueOrDefault()}-{rangeRequest.End.GetValueOrDefault()}");
                }
            }

            return downloadQuery as StreamQuery;
        }
    }
}
