using System;
using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    internal interface IBufferSnapshotBatcher
    {
        IEnumerable<ArraySegment<BufferSnapshot>> Batch(BufferSnapshot[] snapshots);
    }

    internal class BufferSnapshotBatcher : IBufferSnapshotBatcher
    {
        private readonly int maximumBatchSize;

        public BufferSnapshotBatcher(int maximumBatchSize) =>
            this.maximumBatchSize = maximumBatchSize;

        public IEnumerable<ArraySegment<BufferSnapshot>> Batch(BufferSnapshot[] snapshots)
        {
            Array.Sort(snapshots, (a, b) => b.State.Length.CompareTo(a.State.Length));

            var offset = 0;
            var batchSize = 0;
            var i = 0;

            for (; i < snapshots.Length; i++)
            {
                var recordsLength = snapshots[i].State.LengthOfRecords;

                if (batchSize + recordsLength > maximumBatchSize - Buffer.InitialPosition)
                {
                    yield return CreateSegment();
                    offset = i;
                    batchSize = 0;
                }

                batchSize += recordsLength;
            }

            if (batchSize > 0)
                yield return CreateSegment();

            ArraySegment<BufferSnapshot> CreateSegment() =>
                new ArraySegment<BufferSnapshot>(snapshots, offset, i - offset);
        }
    }
}