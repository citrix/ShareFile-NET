using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
#if !Async
using ShareFile.Api.Client.Extensions.Tasks;
#endif

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
            this.bufferSize = UploaderBase.MaxBufferLength;
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
                var bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length);
                while (bytesRead > 0)
                {
#if Async
                    await stream.WriteAsync(buffer, 0, bytesRead);
#else
                    stream.WriteAsync(buffer, 0, bytesRead).WaitForTask();
#endif

                    if (this.progressCallback != null)
                    {
                        this.progressCallback(bytesRead);
                    }

                    totalBytesRead += bytesRead;
                    bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length);
                }
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = this.content.Length;
            return true;
        }
    }

    internal class ByteArrayContentWithProgress : ByteArrayContent
    {
        private readonly byte[] content;
        private readonly int bufferSize;
        private readonly Action<int> progressCallback;


        public ByteArrayContentWithProgress(byte[] content, Action<int> progressCallback)
            : base(content)
        {
            this.content = content;
            this.bufferSize = UploaderBase.MaxBufferLength;
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
#if Async
            async
#endif
            Task SerializeToStreamAsync(Stream stream, TransportContext context)
        {
            for (var totalBytesRead = 0; totalBytesRead < this.content.Length; totalBytesRead += bufferSize)
            {
#if Async
                await stream.WriteAsync(this.content, totalBytesRead, Math.Min(bufferSize, this.content.Length - totalBytesRead));
#else
                stream.WriteAsync(this.content, totalBytesRead, Math.Min(bufferSize, this.content.Length - totalBytesRead)).WaitForTask();
#endif
                if (progressCallback != null)
                {
                    progressCallback(totalBytesRead);
                }
            }
        }

        protected override bool TryComputeLength(out long length)
        {
            length = this.content.Length;
            return true;
        }
    }
}