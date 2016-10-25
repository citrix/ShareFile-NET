using System;
using System.Collections.Generic;

namespace ShareFile.Api.Client.Transfers.Uploaders
{
    internal class CompletedBytes
    {
        private class ByteRange
        {
            public long Offset { get; private set; }
            public long Length { get; private set; }

            public long End
            {
                get
                {
                    return Offset + Length;
                }
            }

            public ByteRange(long offset, long length)
            {
                Offset = offset;
                Length = length;
            }

            public bool CanAppend(ByteRange other)
            {
                return End == other.Offset;
            }

            public void Append(ByteRange other)
            {
                if (!CanAppend(other))
                {
                    throw new InvalidOperationException();
                }
                Length += other.Length;
            }
        }

        private readonly LinkedList<ByteRange> completedRanges;

        public CompletedBytes()
        {
            completedRanges = new LinkedList<ByteRange>();
            completedRanges.AddFirst(new ByteRange(0, 0));
        }

        public void Add(long offset, long length)
        {
            var insertAfter = completedRanges.First;
            while (insertAfter.Next != null && insertAfter.Next.Value.Offset < offset)
            {
                insertAfter = insertAfter.Next;
            }
            var inserted = InsertOrMerge(insertAfter, new ByteRange(offset, length));
            if (inserted.Next != null && inserted.Value.CanAppend(inserted.Next.Value))
            {
                inserted.Value.Append(inserted.Next.Value);
                inserted.List.Remove(inserted.Next);
            }
        }

        private LinkedListNode<ByteRange> InsertOrMerge(LinkedListNode<ByteRange> insertAfter, ByteRange range)
        {
            if (insertAfter.Value.CanAppend(range))
            {
                insertAfter.Value.Append(range);
                return insertAfter;
            }
            return insertAfter.List.AddAfter(insertAfter, range);
        }

        public long CompletedThroughPosition
        {
            get
            {
                return completedRanges.First.Value.End;
            }
        }
    }
}
