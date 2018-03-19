using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using FluentAssertions;
using ShareFile.Api.Client.Transfers.Uploaders.Buffers;
using System.IO;
using System.Buffers;

namespace ShareFile.Api.Client.Tests.Transfers
{
    [TestFixture]
    public class NoncontiguousMemoryStreamTests
    {
        [TestCase]
        public void NcmStream_WriteMatchesRead()
        {
            var rng = new Random(Seed: 5);
            for(int i=0; i<1000; i++)
            {
                const int maxLength = 128 * 1024;
                var inBytes = new byte[rng.Next(maxLength)];
                rng.NextBytes(inBytes);

                var buffers = new PooledBufferAllocator().AllocateBlocks(inBytes.Length);
                new NoncontiguousMemoryStream(buffers).Write(inBytes, 0, inBytes.Length);

                var outBytes = new byte[inBytes.Length];
                int read = new NoncontiguousMemoryStream(buffers).Read(outBytes, 0, outBytes.Length);

                read.Should().Be(outBytes.Length);
                bool eq = Enumerable.SequenceEqual(inBytes, outBytes);
                eq.Should().BeTrue();

                foreach (var b in buffers)
                    ArrayPool<byte>.Shared.Return(b.Array);
            }
        }
        
        [TestCase]
        public void NcmStream_EmptyBufferList_Read()
        {
            var buffers = new List<ArraySegment<byte>>();
            var stream = new NoncontiguousMemoryStream(buffers);
            int read = stream.Read(new byte[1024], 0, 1024);
            read.Should().Be(0);
        }

        [TestCase]
        public void NcmStream_ZeroLengthBuffer_Read()
        {
            var buffers = BufferHelper("", "hello");
            var stream = new NoncontiguousMemoryStream(buffers);

            var outBytes = new byte[1024];
            int read = stream.Read(outBytes, 0, outBytes.Length);

            read.Should().Be(5);
            Encoding.ASCII.GetString(outBytes, 0, read).Should().Be("hello");
        }

        private List<ArraySegment<byte>> BufferHelper(params string[] contents)
        {
            var buffers = new List<ArraySegment<byte>>(capacity: contents.Length);
            foreach(var b in contents)
            {
                buffers.Add(new ArraySegment<byte>(Encoding.ASCII.GetBytes(b)));
            }
            return buffers;
        }

        private List<ArraySegment<byte>> BufferHelper(params int[] lengths)
        {
            var rng = new Random(Seed: 12);
            var buffers = new List<ArraySegment<byte>>(capacity: lengths.Length);
            foreach(int i in lengths)
            {
                var b = new byte[i];
                rng.NextBytes(b);
                buffers.Add(new ArraySegment<byte>(b));
            }
            return buffers;
        }
        
        [TestCase(10, 5)]
        [TestCase(10, 10)]
        [TestCase(5, 10)]
        public void NcmStream_ReadBufferSize(int blockSize, int readBufferSize)
        {
            var buffers = BufferHelper(blockSize);
            var stream = new NoncontiguousMemoryStream(buffers);

            int read = stream.Read(new byte[readBufferSize], 0, readBufferSize);

            read.Should().Be(Math.Min(blockSize, readBufferSize));
        }

        [TestCase(10, 5)]
        [TestCase(10, 10)]
        [TestCase(5, 10)]
        public void NcmStream_WriteBufferSize(int blockSize, int writeBufferSize)
        {
            var buffers = BufferHelper(blockSize);
            var stream = new NoncontiguousMemoryStream(buffers);

            try
            {
                stream.Write(new byte[writeBufferSize], 0, writeBufferSize);
                writeBufferSize.Should().BeLessOrEqualTo(blockSize);
            }
            catch
            {
                writeBufferSize.Should().BeGreaterThan(blockSize);
            }
        }
        
        [TestCase(4)]
        [TestCase(6)]
        public void NcmStream_GetPosition(int count)
        {
            var stream = new NoncontiguousMemoryStream(BufferHelper("share", "file"));
            stream.Read(new byte[count], 0, count);
            stream.Position.Should().Be(count);
        }

        [TestCase]
        public void NcmStream_SeekBegin()
        {
            var stream = new NoncontiguousMemoryStream(BufferHelper("share", "file"));
            stream.Read(new byte[4], 0, 4);

            stream.Seek(0, SeekOrigin.Begin);
            string s = new StreamReader(stream).ReadToEnd();
            s.Should().Be("sharefile");
        }
        
        [TestCase]
        public void NcmStream_SeekCurrent_Forward()
        {
            var stream = new NoncontiguousMemoryStream(BufferHelper("share", "file"));
            stream.Read(new byte[4], 0, 4);

            stream.Seek(2, SeekOrigin.Current);
            string s = new StreamReader(stream).ReadToEnd();
            s.Should().Be("ile");
        }

        [TestCase]
        public void NcmStream_SeekCurrent_Back()
        {
            var stream = new NoncontiguousMemoryStream(BufferHelper("share", "file"));
            stream.Read(new byte[4], 0, 4);

            stream.Seek(-2, SeekOrigin.Current);
            string s = new StreamReader(stream).ReadToEnd();
            s.Should().Be("arefile");
        }
        
        [TestCase]
        public void NcmStream_SeekEnd()
        {
            var stream = new NoncontiguousMemoryStream(BufferHelper("share", "file"));
            stream.Read(new byte[4], 0, 4);

            stream.Seek(-2, SeekOrigin.End);
            string s = new StreamReader(stream).ReadToEnd();
            s.Should().Be("le");
        }

        [TestCase]
        public void NcmStream_BufferOffsetNonzero()
        {
            var buffers = new List<ArraySegment<byte>>
            {
                new ArraySegment<byte>(Encoding.ASCII.GetBytes("share")),
                new ArraySegment<byte>(Encoding.ASCII.GetBytes("pad_file"), 4, 4),
            };
            var stream = new NoncontiguousMemoryStream(buffers);

            var b = new byte[6];
            stream.Read(b, 0, 6);
            Encoding.ASCII.GetString(b).Should().Be("sharef");
            string s = new StreamReader(stream).ReadToEnd();
            s.Should().Be("ile");
        }

        [TestCase]
        public void NcmStream_ReadBuffer_OffsetNonzero()
        {
            var stream = new NoncontiguousMemoryStream(BufferHelper("share", "file"));
            var outBuffer = Encoding.ASCII.GetBytes("aaa123456789");

            stream.Read(outBuffer, 3, outBuffer.Length - 3);

            string s = Encoding.ASCII.GetString(outBuffer);
            s.Should().Be("aaasharefile");
        }

        [TestCase]
        public void NcmStream_WriteBuffer_OffsetNonzero()
        {
            var buffers = BufferHelper(9);
            var stream = new NoncontiguousMemoryStream(buffers);
            var inBuffer = Encoding.ASCII.GetBytes("aaasharefile");

            stream.Write(inBuffer, 3, inBuffer.Length - 3);

            string s = Encoding.ASCII.GetString(buffers[0].Array);
            s.Should().Be("sharefile");
        }
    }
}
