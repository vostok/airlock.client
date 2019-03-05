using System;
using System.Collections.Generic;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal class BufferSnapshotBatcher : IBufferSnapshotBatcher
    {
        private readonly int maximumBatchSize;

        public BufferSnapshotBatcher(int maximumBatchSize) =>
            this.maximumBatchSize = maximumBatchSize;

        public IEnumerable<ArraySegment<BufferSnapshot>> Batch(BufferSnapshot[] snapshots)
        {
            Array.Sort(snapshots, (a, b) => b.State.Length.CompareTo(a.State.Length));

            var firstSnapshot = 0;
            var currentSnapshot = 0;
            var batchSize = 0;

            for (; currentSnapshot < snapshots.Length; currentSnapshot++)
            {
                var recordsLength = snapshots[currentSnapshot].State.Length;

                if (batchSize + recordsLength > maximumBatchSize)
                {
                    if (batchSize > 0)
                        yield return CreateSegment();
                    firstSnapshot = currentSnapshot;
                    batchSize = 0;
                }

                batchSize += recordsLength;
            }

            if (batchSize > 0)
                yield return CreateSegment();

            ArraySegment<BufferSnapshot> CreateSegment() =>
                new ArraySegment<BufferSnapshot>(snapshots, firstSnapshot, currentSnapshot - firstSnapshot);
        }
    }
}