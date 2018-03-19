using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Models;
using System.Runtime.ExceptionServices;
using ShareFile.Api.Client.Extensions;
using ShareFile.Api.Client.Requests;
using System.Buffers;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public class AsyncFileDownloader : AsyncDownloaderBase
    {
        public AsyncFileDownloader(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base(item, client, config)
        { }

        public AsyncFileDownloader(DownloadSpecification downloadSpecification, IShareFileClient client, DownloaderConfig config = null)
            : base(downloadSpecification, client, config)
        { }

        protected override async Task InternalDownloadAsync(Stream outputStream, RangeRequest rangeRequest, CancellationToken cancellationToken)
        {
            var streamQuery = CreateDownloadStreamQuery(rangeRequest);
            Stream downloadStream = null;
            try
            {
                downloadStream = await streamQuery.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                downloadStream = ExpectedLengthStream(downloadStream);
                await ReadAllBytesAsync(outputStream, downloadStream, cancellationToken).ConfigureAwait(false);
            }
            finally
            {
                downloadStream?.Dispose();
            }
        }


#if PORTABLE || NETSTANDARD1_3
        private async Task ReadAllBytesAsync(Stream fileStream, Stream stream, CancellationToken cancellationToken)
        {
            var buffer = new byte[Configuration.BufferSize];

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
                            progressReporter.ReportProgress(bytesRead);
                            await TryPauseAsync(cancellationToken).ConfigureAwait(false);
                        }

                    }
                    while (bytesRead > 0);
                }
            }
        }
#else
        private async Task ReadAllBytesAsync(Stream fileStream, Stream stream, CancellationToken cancellationToken)
        {
            if (stream.CanTimeout)
            {
                stream.ReadTimeout = Client.Configuration.HttpTimeout;
            }

            var timeoutTimer = System.Diagnostics.Stopwatch.StartNew();
            var cancellationTaskSource = new TaskCompletionSource<bool>();
            var cancellationCallbackRegistration = cancellationToken.Register(CancellationCallback, cancellationTaskSource);
            var buffer = ArrayPool<byte>.Shared.Rent(Configuration.BufferSize);
            try
            {
                var timeoutTask = TimeoutChecker(timeoutTimer);
                var cancellationTask = cancellationTaskSource.Task;
                int bytesRead;
                do
                {
                    // NetworkStream.ReadAsync does not respect cancellationToken or readTimeout.
                    var readTask = stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                    // Wait until either the read finishes, the timeout is reached, or the user cancels the download.
                    var finishedTask = await Task.WhenAny(readTask, timeoutTask, cancellationTask).ConfigureAwait(false);
                    if(finishedTask != readTask)
                    {
                        // Download timed out or cancelled, but the read is still running. If it throws, swallow the exception.
                        WaitForFinalRead(readTask, buffer);
                        // The read is still using the buffer. Don't release it until the read finishes.
                        buffer = null;
                        throw finishedTask == timeoutTask ? new TimeoutException() : (Exception)new TaskCanceledException();
                    }
                    // Read finished (but may throw on await).
                    bytesRead = await readTask.ConfigureAwait(false);
                    if (bytesRead > 0)
                    {
                        await fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken).ConfigureAwait(false);
                        progressReporter.ReportProgress(bytesRead);
                        await TryPauseAsync(cancellationToken).ConfigureAwait(false);
                    }
                    timeoutTimer.Restart();
                }
                while (bytesRead > 0);
            }
            finally
            {
                if (buffer != null)
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
                cancellationCallbackRegistration.Dispose();
                timeoutTimer.Stop();
            }
        }

        private static void CancellationCallback(object cancellationTaskSource)
        {
            ((TaskCompletionSource<bool>)cancellationTaskSource).SetResult(result: true);
        }

        private static void WaitForFinalRead(Task readTask, byte[] buffer)
        {
            Task.Run(async () =>
            {
                try
                {
                    await readTask.ConfigureAwait(false);
                }
                catch { }
                finally
                {
                    ArrayPool<byte>.Shared.Return(buffer);
                }
            }).ConfigureAwait(false);
        }

        private async Task TimeoutChecker(System.Diagnostics.Stopwatch timeoutTimer)
        {
            TimeSpan readTimeout = TimeSpan.FromMilliseconds(Client.Configuration.HttpTimeout);
            while(timeoutTimer.IsRunning)
            {
                TimeSpan timeSinceLastRead = timeoutTimer.Elapsed;
                if(timeSinceLastRead > readTimeout)
                {
                    return;
                }
                await Task.Delay(readTimeout - timeSinceLastRead);
            }
        }
#endif
    }
}
