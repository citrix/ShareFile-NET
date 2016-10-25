using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class StreamContentWithProgress : StreamContent
    {
        private readonly Stream content;
        private readonly int bufferSize;
        private readonly Action<long> progressCallback;
        private readonly CancellationToken cancellationToken;

        public StreamContentWithProgress(Stream content, Action<long> progressCallback, CancellationToken cancellationToken = default(CancellationToken))
            : base(content)
        {
            this.content = content;
            this.bufferSize = UploaderBase.DefaultBufferLength;
            this.progressCallback = progressCallback;
            this.cancellationToken = cancellationToken;
        }

        public StreamContentWithProgress(Stream content, Action<long> progressCallback, int bufferSize, CancellationToken cancellationToken = default(CancellationToken))
            : base(content, bufferSize)
        {
            this.content = content;
            this.bufferSize = bufferSize;
            this.progressCallback = progressCallback;
            this.cancellationToken = cancellationToken;
        }

#if ASYNC
        public Func<CancellationToken?, Task> TryPauseAction { get; set; }
#else
        public Action TryPauseAction { get; set; }
#endif

        protected override 
#if ASYNC
            async 
#endif
            Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = new byte[this.bufferSize];
            for (var i = 0; i < this.content.Length; i += this.bufferSize)
            {
                var totalBytesRead = 0;
                var bytesRead = content.Read(buffer, 0, buffer.Length);
                while (bytesRead > 0)
                {
#if ASYNC
                    await stream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                    if (TryPauseAction != null)
                    {
                        await TryPauseAction(cancellationToken).ConfigureAwait(false);
                    }
#else
                    stream.Write(buffer, 0, bytesRead);
                    if (TryPauseAction != null)
                    {
                        TryPauseAction();
                    }
#endif

                    if (this.progressCallback != null)
                    {
                        this.progressCallback(bytesRead);
                    }

                    totalBytesRead += bytesRead;
                    if (bytesRead == buffer.Length)
                    {
                        bytesRead = content.Read(buffer, 0, buffer.Length);
                    }
                    else
                    {
                        break;
                    }
                }
            }

#if !ASYNC
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(0);
            return tcs.Task;
#endif
        }

        protected override bool TryComputeLength(out long length)
        {
            length = this.content.Length;
            return true;
        }
    }
}