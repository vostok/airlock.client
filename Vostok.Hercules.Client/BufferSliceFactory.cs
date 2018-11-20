using System;
using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    internal class BufferSliceFactory : IBufferSliceFactory
    {
        private readonly int maxSliceSize;

        public BufferSliceFactory(int maxSliceSize)
        {
            this.maxSliceSize = maxSliceSize;
        }

        // slices should contain only complete records
        // good: [record][record][record]
        // bad:  [record][record][half-of-rec...
        public IEnumerable<BufferSlice> Cut(BufferSnapshot snapshot)
        {
            if (snapshot.RecordsCount == 0)
                yield break;

            var currentOffset = 0;
            var currentLength = 0;
            var currentCount = 0;

            var position = 0;

            for (var i = 0; i < snapshot.RecordsCount; i++)
            {
                var recordLength = snapshot.Parent.GetRecordSize(position);
                position += recordLength;

                if (recordLength > maxSliceSize)
                    throw new InvalidOperationException($"Encountered a record with length {recordLength} greater than maximum buffer slice size {maxSliceSize}");

                if (currentLength + recordLength > maxSliceSize)
                {
                    yield return new BufferSlice(snapshot.Parent, snapshot.Buffer, currentOffset, currentLength, currentCount);
                    currentOffset += currentLength;
                    currentLength = 0;
                    currentCount = 0;
                }

                currentLength += recordLength;
                currentCount++;
            }

            if (currentLength > 0)
                yield return new BufferSlice(snapshot.Parent, snapshot.Buffer, currentOffset, currentLength, currentCount);
        }
    }
}