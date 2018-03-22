using System;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests;
using ShareFile.Api.Client.Models;
using System.Threading.Tasks;
using System.Threading;
using io = System.IO;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public abstract class DownloaderBase : TransfererBase
    {
        internal Item Item { get; set; }
        internal IShareFileClient Client { get; set; }
        public DownloaderConfig Config { get; set; }
        protected DownloadSpecification DownloadSpecification { get; set; }

        protected TransferProgressReporter progressReporter;
        public event EventHandler<TransferEventArgs> OnTransferProgress
        {
            add { progressReporter.OnTransferProgress += value; }
            remove { progressReporter.OnTransferProgress -= value; }
        }        

        protected DownloaderBase(Item item, IShareFileClient client, DownloaderConfig config = null)
        {
            Client = client;
            Item = item ?? throw new ArgumentNullException(nameof(item));
            Config = config ?? new DownloaderConfig();
            progressReporter = new TransferProgressReporter(
                fileSize: BytesToDownload() ?? 0,
                transferId: "",
                reportInterval: Config.ProgressReportInterval);
        }

        protected DownloaderBase(DownloadSpecification downloadSpecification, IShareFileClient client, DownloaderConfig config = null)
        {
            Client = client;
            Config = config ?? new DownloaderConfig();
            DownloadSpecification = downloadSpecification ?? throw new ArgumentNullException(nameof(downloadSpecification));
            progressReporter = new TransferProgressReporter(
                fileSize: BytesToDownload() ?? 0,
                transferId: "",
                reportInterval: Config.ProgressReportInterval);
        }

        protected long? BytesToDownload()
        {
            // math operators on nullables return null if an arg is null, comparison operators return false
            long begin = Config.RangeRequest?.Begin ?? 0;
            long? end = Config.RangeRequest?.End ?? (Item?.FileSizeBytes - 1);
            long? length = end - begin + 1; // inclusive range
            if(length < 0)
            {
                throw new ArgumentException($"Cannot download negative-length byte range {begin} : {end}");
            }
            return length;
        }

        protected bool IsRangeRequest => Config.RangeRequest != null && (Config.RangeRequest.Begin.HasValue || Config.RangeRequest.End.HasValue);

        protected io.Stream ExpectedLengthStream(io.Stream downloadStream)
        {
            long? expectedBytes = BytesToDownload();
            if (!expectedBytes.HasValue)
            {
                return downloadStream;
            }
            bool allowRangeReqOffByOne = IsRangeRequest && Config.AllowRangeRequestOffByOne;
            return new ExpectedLengthStream(downloadStream, expectedBytes.Value, allowRangeReqOffByOne, Client.Configuration.Logger);
        }

        protected StreamQuery CreateDownloadQuery(RangeRequest rangeRequest)
        {
            return CreateDownloadStreamQuery(Config.RangeRequest);
        }
		
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
