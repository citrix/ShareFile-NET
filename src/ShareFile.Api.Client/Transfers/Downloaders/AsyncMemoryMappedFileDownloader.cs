#if !PORTABLE && !NETSTANDARD1_3
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ShareFile.Api.Client.Models;
using System.IO.MemoryMappedFiles;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    public class AsyncMemoryMappedFileDownloader : AsyncDownloaderBase, IDisposable
    {
        private readonly long fileSize;
        private readonly object locker = new object();
        private MemoryMappedFile memoryMappedFile;
        private long completedBytes = 0;
        private int readStreamsInUse = 0;
        private readonly LinkedList<ReadRequest> readRequests = new LinkedList<ReadRequest>();
        private bool disposed = false;

        public AsyncMemoryMappedFileDownloader(Item item, IShareFileClient client, DownloaderConfig config = null)
            : base(item, client, config)
        {
            if (Config.RangeRequest != null)
                throw new ArgumentException($"{nameof(AsyncMemoryMappedFileDownloader)} does not support {nameof(RangeRequest)}", $"{nameof(DownloaderConfig)}");
            if (!item.FileSizeBytes.HasValue)
                throw new ArgumentException($"{nameof(Item)} must include {nameof(Item.FileSizeBytes)}", $"{nameof(Item)}");

            fileSize = item.FileSizeBytes.Value;
        }

        private MemoryMappedFile CreateMemoryMappedFile(FileStream fileStream)
        {
            return MemoryMappedFile.CreateFromFile(
                fileStream,
                mapName: null,
                capacity: fileSize,
                access: MemoryMappedFileAccess.ReadWrite,
                inheritability: HandleInheritability.None,
                leaveOpen: false);
        }

        protected override async Task InternalDownloadAsync(Stream stream, RangeRequest rangeRequest, CancellationToken cancellationToken)
        {
            if (!(stream is FileStream))
                throw new ArgumentException($"Stream argument to {nameof(AsyncMemoryMappedFileDownloader)} must be a {nameof(FileStream)}");
            
            using (var inputStream = await DownloadStreamAsync(cancellationToken).ConfigureAwait(false))
            {
                memoryMappedFile = CreateMemoryMappedFile((FileStream)stream);
                using (var outputStream = memoryMappedFile.CreateViewStream(0, fileSize))
                {
                    var readTask = inputStream.CopyToAsync(new ProgressStream(outputStream, OnProgress));
                    var cancelCheckTask = CancellationCheckerAsync(cancellationToken, TimeSpan.FromSeconds(1));
                    await Task.WhenAny(readTask, cancelCheckTask).ConfigureAwait(false);
                }
                if (cancellationToken.IsCancellationRequested)
                    throw new TaskCanceledException();
            }
        }

        private async Task<Stream> DownloadStreamAsync(CancellationToken cancellationToken)
        {
            var streamQuery = CreateDownloadStreamQuery(rangeRequest: null);
            var stream = await streamQuery.ExecuteAsync(cancellationToken).ConfigureAwait(false);
            if (stream.CanTimeout)
                stream.ReadTimeout = Client.Configuration.HttpTimeout;

            return stream;
        }

        private async Task CancellationCheckerAsync(CancellationToken cancellationToken, TimeSpan cancellationCheckInterval)
        {
            while (!cancellationToken.IsCancellationRequested)
                await Task.Delay(cancellationCheckInterval);
        }

        private void OnProgress(long bytesTransferred)
        {
            lock (locker)
            {
                completedBytes += bytesTransferred;
                CompleteReadRequests();
            }
            progressReporter.ReportProgress(bytesTransferred);
        }

        public Task<Stream> ReadFileAsync(long offset, long size)
        {
            lock(locker)
            {
                if (disposed)
                    throw new ObjectDisposedException(nameof(AsyncMemoryMappedFileDownloader));

                if (offset < 0 || offset + size > fileSize)
                    throw new ArgumentException($"Invalid {nameof(ReadFileAsync)} args: got {offset}:{offset + size}, accept {0}:{fileSize}");

                if (offset + size <= completedBytes)
                    return Task.FromResult(CreateReadStream(offset, size));

                return EnqueueReadRequest(offset, size).Waiter.Task;
            }
        }

        private Stream CreateReadStream(long offset, long size)
        {
            var stream = memoryMappedFile.CreateViewStream(offset, size, MemoryMappedFileAccess.Read);
            return new ReadStream(this, stream);
        }

        private struct ReadRequest
        {
            public long Offset;
            public long Size;
            public TaskCompletionSource<Stream> Waiter;
        }
        
        private ReadRequest EnqueueReadRequest(long offset, long size)
        {
            var readRequest = new ReadRequest
            {
                Offset = offset,
                Size = size,
                Waiter = new TaskCompletionSource<Stream>(),
            };
            readRequests.AddFirst(readRequest);
            return readRequest;
        }

        private void CompleteReadRequests()
        {
            LinkedListNode<ReadRequest> node = readRequests.First;
            LinkedListNode<ReadRequest> next = null;
            while (node != null)
            {
                next = node.Next;
                var readRequest = node.Value;
                bool canComplete = readRequest.Offset + readRequest.Size <= completedBytes;
                if (canComplete)
                {
                    readRequests.Remove(node);
                    var readStream = CreateReadStream(readRequest.Offset, readRequest.Size);
                    Task.Run(() => readRequest.Waiter.SetResult(readStream));
                }
                node = next;
            }
        }

        public void Dispose()
        {
            if (disposed)
                return;

            lock (locker)
            {
                foreach (var readRequest in readRequests)
                {
                    Task.Run(() => readRequest.Waiter.SetException(new ObjectDisposedException(nameof(AsyncMemoryMappedFileDownloader))));
                }

                TimeSpan maxWaitForReadStreams = TimeSpan.FromSeconds(1);
                TimeSpan spinInterval = TimeSpan.FromMilliseconds(20);
                for(var waited = TimeSpan.Zero; readStreamsInUse > 0 && waited < maxWaitForReadStreams; waited += spinInterval)
                {
                    Monitor.Exit(locker);
                    Thread.Sleep(spinInterval);
                    Monitor.Enter(locker);
                }
                if (readStreamsInUse > 0)
                    throw new TimeoutException($"{readStreamsInUse} read streams still in use after {maxWaitForReadStreams}");

                memoryMappedFile?.Dispose();
                disposed = true;
            }
        }

        private class ReadStream : StreamWrapper
        {
            private AsyncMemoryMappedFileDownloader downloader;
            private bool closed = false;

            public ReadStream(AsyncMemoryMappedFileDownloader downloader, Stream stream)
                : base(stream)
            {
                this.downloader = downloader;
                lock(downloader.locker)
                {
                    downloader.readStreamsInUse += 1;
                }
            }

            public override void Close()
            {
                if (!closed)
                {
                    lock (downloader.locker)
                    {
                        downloader.readStreamsInUse -= 1;
                    }
                    closed = true;
                }
                base.Close();
            }
        }
    }
}
#endif