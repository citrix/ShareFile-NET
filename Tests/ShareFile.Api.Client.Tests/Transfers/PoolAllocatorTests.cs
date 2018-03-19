using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using ShareFile.Api.Client.Transfers.Uploaders.Buffers;
using System.Buffers;

namespace ShareFile.Api.Client.Tests.Transfers
{
    [TestFixture]
    public class PoolAllocatorTests
    {
        [TestCase]
        public void PoolAlloc_AllocationSize()
        {
            var rng = new Random(Seed: 5);
            for(int i=0; i<10000; i++)
            {
                const int maxLength = 10 * 1024 * 1024;
                int expectedLength = rng.Next(maxLength);
                List<ArraySegment<byte>> buffers = new PooledBufferAllocator().AllocateBlocks(expectedLength);
                int actualLength = buffers.Sum(b => b.Count);
                actualLength.Should().Be(expectedLength);
                foreach (var b in buffers)
                    ArrayPool<byte>.Shared.Return(b.Array);
            }
        }

        [TestCase]
        public void PoolAlloc_Zero()
        {
            List<ArraySegment<byte>> buffers = new PooledBufferAllocator().AllocateBlocks(length: 0);
            int length = buffers.Sum(b => b.Count);
            length.Should().Be(0);
        }
    }
}
