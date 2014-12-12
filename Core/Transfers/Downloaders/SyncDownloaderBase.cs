using System.Collections.Generic;
using System.IO;
using ShareFile.Api.Models;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public abstract class SyncDownloaderBase : DownloaderBase
    {
        protected SyncDownloaderBase(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base(item, client, config)
        {
        }

        public abstract void DownloadTo(Stream fileStream, Dictionary<string, object> transferMetadata = null);
    }
}
