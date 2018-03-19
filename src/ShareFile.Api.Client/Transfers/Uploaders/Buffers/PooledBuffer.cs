using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShareFile.Api.Client.Transfers.Uploaders.Buffers
{
    public interface IBuffer : IDisposable
    {
        int Length { get; }
        Stream GetStream();
    }

    internal class PooledBuffer : IBuffer
    {
        public int Length { get; }
        private readonly List<ArraySegment<byte>> blocks;
        private readonly ArrayPool<byte> pool;
        private bool disposed = false;

        public PooledBuffer(ArrayPool<byte> pool, List<ArraySegment<byte>> blocks)
        {
            this.blocks = blocks;
            this.pool = pool;
            Length = blocks.Sum(b => b.Count);
        }

        public Stream GetStream()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(PooledBuffer));
            return new NoncontiguousMemoryStream(blocks);
        }

        public void Dispose()
        {
            if (disposed)
                return;

            foreach (var arraySegment in blocks)
            {
                pool.Return(arraySegment.Array);
            }
            blocks.Clear();
            disposed = true;
        }
    }
}
