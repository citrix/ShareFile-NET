using ShareFile.Api.Client.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Downloaders
{
    internal class ExpectedLengthStream : StreamWrapper
    {
        private readonly long expectedLength;
        private long totalBytesRead = 0;

        private readonly bool allowOffByOne;
        private readonly ILogger logger;

        public ExpectedLengthStream(Stream stream, long expectedLength, bool allowOffByOne, ILogger logger)
            : base(stream)
        {
            this.expectedLength = expectedLength;
            this.allowOffByOne = allowOffByOne;
            this.logger = logger;
        }

        private void OnRead(int bytesRead)
        {
            bool endOfStream = bytesRead == 0;
            if (endOfStream)
            {
                if (allowOffByOne && totalBytesRead == expectedLength - 1)
                {
                    logger?.Error($"Download expected {expectedLength} bytes but stream only contained {totalBytesRead} bytes. Check for off-by-one on RangeRequest (range is inclusive).");
                }
                else if (totalBytesRead < expectedLength)
                {
                    throw new Exception($"Expected {expectedLength} bytes but stream only contained {totalBytesRead} bytes.");
                }
                return;
            }
            totalBytesRead += bytesRead;
            if (totalBytesRead > expectedLength)
            {
                throw new Exception($"Expected {expectedLength} bytes but stream contains at least {totalBytesRead} bytes.");
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = base.Read(buffer, offset, count);
            OnRead(read);
            return read;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read = await base.ReadAsync(buffer, offset, count, cancellationToken);
            OnRead(read);
            return read;
        }
    }
}