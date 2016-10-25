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
        private readonly int bufferSize;
        private readonly Action<int> progressCallback;


        public ByteArrayContentWithProgress(byte[] content, Action<int> progressCallback)
            : base(content)
        {
            this.content = content;
            this.bufferSize = UploaderBase.DefaultBufferLength;
            this.progressCallback = progressCallback;
        }

        public ByteArrayContentWithProgress(byte[] content, Action<int> progressCallback, int offset, int count, int bufferSize)
            : base(content, offset, count)
        {
            this.content = content;
            this.bufferSize = bufferSize;
            this.progressCallback = progressCallback;
        }

        protected override 
#if ASYNC
            async
#endif
            Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            
            for (var totalBytesRead = 0; totalBytesRead < this.content.Length; totalBytesRead += this.bufferSize)
            {
                var bytesToWrite = Math.Min(this.bufferSize, this.content.Length - totalBytesRead);
#if ASYNC
                await stream.WriteAsync(this.content, totalBytesRead, bytesToWrite).ConfigureAwait(false);
#else
                stream.Write(this.content, totalBytesRead, bytesToWrite);
#endif
                if (this.progressCallback != null)
                {
                    this.progressCallback(bytesToWrite);
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