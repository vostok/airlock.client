using System;
using System.Collections.Generic;
using Vostok.Commons.Binary;

namespace Vostok.Airlock.Client
{
    internal class BufferSliceFactory : IBufferSliceFactory
    {
        private readonly int maxSliceSize;

        public BufferSliceFactory(int maxSliceSize)
        {
            this.maxSliceSize = maxSliceSize;
        }

        public IEnumerable<BufferSlice> Cut(BufferSnapshot snapshot)
        {
            var binaryReader = new BinaryBufferReader(snapshot.Buffer);
            
            if (snapshot.BufferPosition <= maxSliceSize)
            {
                yield return new BufferSlice(snapshot.Buffer, 0, snapshot.BufferPosition, snapshot.RecordsCount);
            }

            var currentOffset = 0;
            var currentLength = 0;
            var currentCount = 0;

            for (var i = 0; i < snapshot.RecordsCount; i++)
            {
                var recordLength = binaryReader.ReadInt32();
                binaryReader.Position += recordLength;

                if (recordLength > maxSliceSize)
                {
                    throw new InvalidOperationException();
                }

                if (currentLength + recordLength > maxSliceSize)
                {
                    yield return new BufferSlice(snapshot.Buffer, currentOffset, currentLength, currentCount);
                    currentOffset += currentLength;
                    currentLength = 0;
                    currentCount = 0;
                }

                currentLength += recordLength;
                currentCount++;
            }

            if (currentLength > 0)
            {
                yield return new BufferSlice(snapshot.Buffer, currentOffset, currentLength, currentCount);
            }
        }
    }
}