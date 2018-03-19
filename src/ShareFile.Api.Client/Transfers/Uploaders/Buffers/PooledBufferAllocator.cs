using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders.Buffers
{
    public interface IBufferAllocator
    {
        IBuffer Allocate(int length);
    }

    internal class PooledBufferAllocator : IBufferAllocator
    {
        private const int defaultMaxBlockSize = 1024 * 1024; // do not increase - max pooled size for default array pool
        private const int defaultMinBlockSize = 4 * 1024; // don't go too small - array pool only holds 50 arrays per size

        private readonly ArrayPool<byte> pool;
        private readonly int maxBlockSize;
        private readonly int minBlockSize;

        public PooledBufferAllocator() : this(ArrayPool<byte>.Shared, defaultMaxBlockSize, defaultMinBlockSize) { }

        public PooledBufferAllocator(ArrayPool<byte> pool, int maxBlockSize, int minBlockSize)
        {
            this.pool = pool;
            this.maxBlockSize = maxBlockSize;
            this.minBlockSize = minBlockSize;
        }

        public IBuffer Allocate(int length) => new PooledBuffer(pool, AllocateBlocks(length));

        internal List<ArraySegment<byte>> AllocateBlocks(int length)
        {
            var blocks = new List<ArraySegment<byte>>();
            for (int toAllocate = length; toAllocate > 0;)
            {
                int blockSizeToRequest = maxBlockSize;
                while (toAllocate < blockSizeToRequest && blockSizeToRequest > minBlockSize)
                    blockSizeToRequest >>= 2;

                byte[] array = pool.Rent(blockSizeToRequest);
                if (array.Length < blockSizeToRequest)
                    throw new Exception($"Requested {blockSizeToRequest} bytes from ArrayPool, got {array.Length} bytes");

                ArraySegment<byte> block = array.Length > toAllocate
                    ? new ArraySegment<byte>(array, offset: 0, count: toAllocate)
                    : new ArraySegment<byte>(array);
                blocks.Add(block);
                toAllocate -= block.Count;
            }
            return blocks;
        }
    }
}