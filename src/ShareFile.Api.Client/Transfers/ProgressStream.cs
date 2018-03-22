using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers
{
    internal class ProgressStream : StreamWrapper
    {
        private readonly Action<long> progressCallback;

        public ProgressStream(Stream stream, Action<long> progressCallback)
            : base(stream)
        {
            this.progressCallback = progressCallback;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            progressCallback(read);
            return read;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            base.Write(buffer, offset, count);
            progressCallback(count);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read = await base.ReadAsync(buffer, offset, count, cancellationToken);
            progressCallback(read);
            return read;
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await base.WriteAsync(buffer, offset, count, cancellationToken);
            progressCallback(count);
        }
    }
}
