using System;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
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
