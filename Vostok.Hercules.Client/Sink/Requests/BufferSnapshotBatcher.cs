using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal class BufferSnapshotBatcher : IBufferSnapshotBatcher
    {
        private readonly int maximumBatchSize;

        public BufferSnapshotBatcher(int maximumBatchSize) =>
            this.maximumBatchSize = maximumBatchSize;

        public IEnumerable<IReadOnlyList<BufferSnapshot>> Batch(IEnumerable<BufferSnapshot> snapshots)
        {
            var sortedSnapshots = snapshots
                .OrderByDescending(sn => sn.State.Length)
                .ToArray();

            var firstSnapshot = 0;
            var currentSnapshot = 0;
            var batchSize = 0;

            for (; currentSnapshot < sortedSnapshots.Length; currentSnapshot++)
            {
                var recordsLength = sortedSnapshots[currentSnapshot].State.Length;

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

            IReadOnlyList<BufferSnapshot> CreateSegment() =>
                new ArraySegment<BufferSnapshot>(sortedSnapshots, firstSnapshot, currentSnapshot - firstSnapshot);
        }
    }
}