using System;
using System.Buffers;
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
        private readonly Action<long> progressCallback;
        private readonly CancellationToken cancellationToken;

        public StreamContentWithProgress(Stream content, Action<long> progressCallback, CancellationToken cancellationToken = default(CancellationToken))
            : base(content)
        {
            this.content = content;
            this.progressCallback = progressCallback;
            this.cancellationToken = cancellationToken;
        }
		
        public Func<CancellationToken, Task> TryPauseAction { get; set; }

        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            var buffer = ArrayPool<byte>.Shared.Rent(Configuration.BufferSize);
            try
            {
                for (var i = 0; i < content.Length; i += buffer.Length)
                {
                    var totalBytesRead = 0;
                    var bytesRead = await content.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                    while (bytesRead > 0)
                    {
                        await stream.WriteAsync(buffer, 0, bytesRead).ConfigureAwait(false);
                        if (TryPauseAction != null)
                        {
                            await TryPauseAction(cancellationToken).ConfigureAwait(false);
                        }

                        progressCallback?.Invoke(bytesRead);

                        totalBytesRead += bytesRead;
                        if (bytesRead == buffer.Length)
                        {
                            bytesRead = await content.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = this.content.Length;
            return true;
        }
    }
}