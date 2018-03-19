using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class ByteArrayContentWithProgress : ByteArrayContent
    {
        private readonly byte[] content;
        private readonly Action<int> progressCallback;


        public ByteArrayContentWithProgress(byte[] content, Action<int> progressCallback)
            : base(content)
        {
            this.content = content;
            this.progressCallback = progressCallback;
        }
        
        protected override async Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {            
            for (var totalBytesRead = 0; totalBytesRead < this.content.Length; totalBytesRead += Configuration.BufferSize)
            {
                var bytesToWrite = Math.Min(Configuration.BufferSize, this.content.Length - totalBytesRead);
                await stream.WriteAsync(this.content, totalBytesRead, bytesToWrite).ConfigureAwait(false);

				progressCallback?.Invoke(bytesToWrite);
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = this.content.Length;
            return true;
        }
    }
}