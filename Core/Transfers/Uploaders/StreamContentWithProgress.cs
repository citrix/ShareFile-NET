using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class StreamContentWithProgress : StreamContent
    {
        private readonly Stream content;
        private readonly int bufferSize;
        private readonly Action<int> progressCallback;

        public StreamContentWithProgress(Stream content, Action<int> progressCallback)
            : base(content)
        {
            this.content = content;
            this.bufferSize = UploaderBase.DefaultBufferLength;
            this.progressCallback = progressCallback;
        }

        public StreamContentWithProgress(Stream content, Action<int> progressCallback, int bufferSize)
            : base(content, bufferSize)
        {
            this.content = content;
            this.bufferSize = bufferSize;
            this.progressCallback = progressCallback;
        }

        protected override 
#if Async
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
#if Async
                    await stream.WriteAsync(buffer, 0, bytesRead);
#else
                    stream.Write(buffer, 0, bytesRead);
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
                }
            }

#if !Async
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