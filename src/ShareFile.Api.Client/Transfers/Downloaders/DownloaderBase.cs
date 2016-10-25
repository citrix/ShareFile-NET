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
            Client = client;
            Item = item;
            Config = config ?? new DownloaderConfig();
        }

        protected DownloaderBase(DownloadSpecification downloadSpecification, IShareFileClient client, DownloaderConfig config = null)
        {
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
        protected Task<DownloadSpecification> CreateDownloadSpecificationAsync(CancellationToken token = default(CancellationToken))
        {
            if (Item == null)
            {
                throw new InvalidOperationException("A download specification can not be created.  No item has been specified.");
            }
            
            return Client.Items.Download(Item.GetObjectUri(), false).Expect<DownloadSpecification>().ExecuteAsync(token);
        }
#endif

        protected DownloadSpecification CreateDownloadSpecification()
        {
            if (Item == null)
            {
                throw new InvalidOperationException("A download specification can not be created.  No item has been specified.");
            }

            return Client.Items.Download(Item.GetObjectUri(), false).Expect<DownloadSpecification>().Execute();
        }

        protected StreamQuery CreateDownloadStreamQuery(RangeRequest rangeRequest)
        {
            rangeRequest = rangeRequest ?? (Config == null ? null : Config.RangeRequest);
            var downloadQuery = new StreamQuery(Client).Uri(DownloadSpecification.DownloadUrl);

            downloadQuery.AddHeader("Accept", "*/*");

            if (rangeRequest != null)
            {
                if (!rangeRequest.End.HasValue)
                {
                    downloadQuery.AddHeader("Range", string.Format("bytes={0}-", rangeRequest.Begin.GetValueOrDefault()));
                }
                else
                {
                    downloadQuery.AddHeader("Range",
                        string.Format("bytes={0}-{1}", rangeRequest.Begin.GetValueOrDefault(),
                            rangeRequest.End.GetValueOrDefault()));
                }
            }

            return downloadQuery as StreamQuery;
        }
    }
}
