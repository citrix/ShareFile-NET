using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Models;
using System.Runtime.ExceptionServices;
using ShareFile.Api.Client.Requests;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
#if ASYNC
    public class AsyncFileDownloader : DownloaderBase
    {
        public AsyncFileDownloader(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base (item, client, config)
        {
        }

        public AsyncFileDownloader(DownloadSpecification downloadSpecification, IShareFileClient client, DownloaderConfig config = null)
            : base(downloadSpecification, client, config)
        {
        }

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
        public async virtual Task DownloadToAsync(
            Stream stream,
            CancellationToken? cancellationToken = null,
            Dictionary<string, object> transferMetadata = null,
            RangeRequest rangeRequest = null)
        {
            if (rangeRequest != null && DownloadSpecification == null)
            {
                throw new InvalidOperationException("Downloader instance has not been prepared. In order to supply a RangeRequest, you must first call PrepareDownloadAsync.");
            }

            if (DownloadSpecification == null)
            {
                await PrepareDownloadAsync();
            }

            var streamQuery = CreateDownloadStreamQuery(rangeRequest);
            var totalBytesToDownload = Item.FileSizeBytes.GetValueOrDefault();
            var progress = new TransferProgress(totalBytesToDownload, transferMetadata);

            using (var downloadStream = await streamQuery.ExecuteAsync(cancellationToken))
            {
                if (downloadStream != null)
                {
                    await ReadAllBytesAsync(stream, downloadStream, progress, cancellationToken.GetValueOrDefault()).ConfigureAwait(false);
                }
            }

            NotifyProgress(progress.MarkComplete());
        }


#if PORTABLE || NETSTANDARD1_3
        private async Task ReadAllBytesAsync(Stream fileStream, Stream stream, TransferProgress progress, CancellationToken cancellationToken)
        {
            var buffer = new byte[Config.BufferSize];

            using (var timeoutToken = new CancellationTokenSource())
            {
                // Link the two tokens so that it will either timeout or be user-cancellable
                using (
                    var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(
                        timeoutToken.Token,
                        cancellationToken))
                {
                    int bytesRead;
                    do
                    {
                        timeoutToken.CancelAfter(Client.Configuration.HttpTimeout);

                        bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, linkedToken.Token).ConfigureAwait(false);

                        if (bytesRead > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);

                            NotifyProgress(progress.UpdateBytesTransferred(bytesRead));

                            await TryPauseAsync(cancellationToken).ConfigureAwait(false);
                        }

                    }
                    while (bytesRead > 0);
                }
            }
        }
#else
        private async Task ReadAllBytesAsync(Stream fileStream, Stream stream, TransferProgress progress, CancellationToken cancellationToken)
        {
            if (stream.CanTimeout)
            {
                stream.ReadTimeout = Client.Configuration.HttpTimeout;
            }
            
            using (var linkedToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                try
                {
                    var cancellationTask = CancellationChecker(linkedToken.Token);
                    var buffer = new byte[Config.BufferSize];
                    int bytesRead;
                    do
                    {
                        // We have to use stream.Read instead of stream.ReadyAsync due to a .NET bug
                        var readTask = Task.Factory.StartNew(
                            () => stream.Read(buffer, 0, buffer.Length),
                            cancellationToken,
                            TaskCreationOptions.LongRunning,
                            TaskScheduler.Default);

                        // Wait until either the read finished, or the user cancelled the operation
                        var finishedTask = await Task.WhenAny(readTask, cancellationTask).ConfigureAwait(false);
                        if (finishedTask == readTask)
                        {
                            bytesRead = readTask.Result;
                        }
                        else
                        {
                            throw new TaskCanceledException();
                        }
                        if (bytesRead > 0)
                        {
                            fileStream.Write(buffer, 0, bytesRead);

                            NotifyProgress(progress.UpdateBytesTransferred(bytesRead));

                            await TryPauseAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                    while (bytesRead > 0);
                }
                catch (AggregateException ex)
                {
                    // The read or cancellation task will always be wrapped, so just rethrow the inner exception.
                    throw ex.InnerException;
                }
                finally
                {
                    // Clean up the timer
                    linkedToken.Cancel();
                }
            }
        }

        private Task<bool> CancellationChecker(CancellationToken cancellationToken)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();

            // Timer will poll the cancellation token ever 1000ms
            Timer timer = null;
            timer = new Timer(_ =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.SetCanceled();
                    timer.Dispose();
                }
                else
                {
                    timer.Change(1000, Timeout.Infinite);
                }
            }, null, 1000, Timeout.Infinite);

            return tcs.Task;
        }
#endif
    }
#endif
}
