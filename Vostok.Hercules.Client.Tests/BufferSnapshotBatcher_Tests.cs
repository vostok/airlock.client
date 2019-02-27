using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Hercules.Client.Tests
{
    internal class BufferSnapshotBatcher_Tests
    {
        
        private const int MaxBatchSize = 100;
        private BufferSnapshotBatcher batcher = new BufferSnapshotBatcher(MaxBatchSize);
        
        [Test]
        public void Should_return_single_batch_for_single_snapshot()
        {
            var snapshots = new[] {new BufferSnapshot(null, null, new BufferState(10, 4))};
            var segments = batcher.Batch(snapshots).ToArray();

            segments.Should().HaveCount(1);
            segments[0].Should().BeEquivalentTo(new ArraySegment<BufferSnapshot>(snapshots, 0, snapshots.Length));
        }

        [Test]
        public void Should_merge_small_snapshots_when_all_snapshots_fit_in_one_batch()
        {
            var snapshots = new[]
            {
                new BufferState(10, 4),
                new BufferState(14, 2),
                new BufferState(20, 3)
            }.Select(x => new BufferSnapshot(null, null, x)).ToArray();
            var segments = batcher.Batch(snapshots).ToArray();

            segments.Should().HaveCount(1);
            segments[0].Should().BeEquivalentTo(new ArraySegment<BufferSnapshot>(snapshots, 0, snapshots.Length));
        }

        [Test]
        public void Should_keep_big_snapshots()
        {
            var snapshots = new[]
            {
                new BufferState(10, 4),
                new BufferState(14, 2),
                new BufferState(1000, 2),
                new BufferState(20, 3)
            }.Select(x => new BufferSnapshot(null, null, x)).ToArray();
            var segments = batcher.Batch(snapshots).ToArray();

            segments.Should().HaveCount(2);
        }
        
        [Test]
        public void Should_merge_small_snapshots()
        {
            var snapshotLength = 10;
            var snapshotCount = 200;
            var expectedBatchCount = (snapshotLength - Buffer.InitialPosition) * snapshotCount / (MaxBatchSize - Buffer.InitialPosition) + 1;
            
            var snapshots = Enumerable.Repeat(new BufferState(snapshotLength, 4), snapshotCount).Select(x => new BufferSnapshot(null, null, x)).ToArray();
            var segments = batcher.Batch(snapshots).ToArray();

            segments.Should().HaveCount(expectedBatchCount);
            segments.Sum(x => x.Count).Should().Be(snapshotCount);
        }
    }
}