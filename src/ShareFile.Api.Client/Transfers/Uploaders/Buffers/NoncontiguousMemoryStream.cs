using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders.Buffers
{
    internal class NoncontiguousMemoryStream : Stream
    {
        private readonly IReadOnlyList<ArraySegment<byte>> blocks;

        private int blockIndex = 0;
        private int blockOffset = 0;

        public NoncontiguousMemoryStream(IReadOnlyList<ArraySegment<byte>> blocks)
        {
            this.blocks = blocks;
            Length = blocks.Sum(b => b.Count);
        }

        private ArraySegment<byte> GetCurrentBlockRemainder()
        {
            while (blockIndex < blocks.Count)
            {
                ArraySegment<byte> block = blocks[blockIndex];
                if (block.Count > blockOffset)
                {
                    return new ArraySegment<byte>(block.Array, block.Offset + blockOffset, block.Count - blockOffset);
                }
                blockIndex++;
                blockOffset -= block.Count;
            }
            return new ArraySegment<byte>(ArrayPool<byte>.Shared.Rent(0));
        }
        
        private int Copy(ArraySegment<byte> source, ArraySegment<byte> dest)
        {
            int count = Math.Min(source.Count, dest.Count);
            Buffer.BlockCopy(source.Array, source.Offset, dest.Array, dest.Offset, count);
            blockOffset += count;
            return count;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;

        public override long Length { get; }

        public override long Position
        {
            get
            {
                long pos = 0;
                for (int i = 0; i < blockIndex; i++)
                    pos += blocks[i].Count;
                pos += blockOffset;
                return pos;
            }
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException($"{nameof(Position)} must be non-negative");
                if (value > Length)
                    throw new ArgumentOutOfRangeException($"{nameof(Position)} must be less than or equal to stream length");
                blockIndex = 0;
                blockOffset = (int)value;
            }
        }

        public override void Flush() { return; }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int read = 0;
            while(count > 0)
            {
                ArraySegment<byte> currentBlockRemainder = GetCurrentBlockRemainder();
                if (currentBlockRemainder.Count == 0)
                    break;
                int copied = Copy(
                    source: currentBlockRemainder,
                    dest: new ArraySegment<byte>(buffer, offset, count));
                read += copied;
                offset += copied;
                count -= copied;
            }
            return read;
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(cancellationToken);
            int read = Read(buffer, offset, count);
            return Task.FromResult(read);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset > int.MaxValue || offset < int.MinValue)
                throw new ArgumentOutOfRangeException($"{nameof(offset)}");
            switch (origin)
            {
                case SeekOrigin.Begin:
                    blockIndex = 0;
                    blockOffset = (int)offset;
                    break;
                case SeekOrigin.Current:
                    blockOffset += (int)offset;
                    break;
                case SeekOrigin.End:
                    blockIndex = blocks.Count;
                    blockOffset = (int)offset;
                    break;
                default:
                    throw new ArgumentException($"Unexpected {nameof(SeekOrigin)} value {origin}");
            }
            while(blockOffset < 0)
            {
                blockIndex--;
                if (blockIndex < 0)
                    throw new ArgumentException($"Invalid offset");
                blockOffset += blocks[blockIndex].Count;
            }
            return Position;
        }

        public override void SetLength(long value) => throw new NotSupportedException($"{nameof(NoncontiguousMemoryStream)} does not support reallocation");

        public override void Write(byte[] buffer, int offset, int count)
        {
            while (count > 0)
            {
                ArraySegment<byte> currentBlockRemainder = GetCurrentBlockRemainder();
                if (currentBlockRemainder.Count == 0)
                    throw new Exception("Buffer is full");
                int copied = Copy(
                    source: new ArraySegment<byte>(buffer, offset, count),
                    dest: currentBlockRemainder);
                offset += copied;
                count -= copied;
            }
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                throw new OperationCanceledException(cancellationToken);
            Write(buffer, offset, count);
#if NETSTANDARD_13
            return Task.CompletedTask;
#else
            return Task.FromResult(true);
#endif
        }
    }
}
