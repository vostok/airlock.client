using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Requests;

namespace Vostok.Hercules.Client.Tests.Sink.Requests
{
    [TestFixture]
    internal class BufferSnapshotBatcher_Tests
    {
        private const int MaximumBatchSize = 100;

        private BufferSnapshotBatcher batcher;

        [SetUp]
        public void TestSetup()
        {
            batcher = new BufferSnapshotBatcher(MaximumBatchSize);
        }

        [Test]
        public void Should_produce_no_batches_when_given_no_snapshots()
        {
            batcher.Batch(Snapshots()).Should().BeEmpty();
        }

        [Test]
        public void Should_produce_no_batches_when_given_only_empty_snapshots()
        {
            batcher.Batch(Snapshots(0, 0, 0, 0, 0)).Should().BeEmpty();
        }

        [Test]
        public void Should_produce_batches_of_one_snapshot_when_none_of_them_fit_together()
        {
            var snapshots = Snapshots(99, 52, 60, 51, 80);

            var batches = batcher.Batch(snapshots).ToArray();

            batches.Should().HaveCount(5);
            batches[0].Should().Equal(snapshots[0]);
            batches[1].Should().Equal(snapshots[4]);
            batches[2].Should().Equal(snapshots[2]);
            batches[3].Should().Equal(snapshots[1]);
            batches[4].Should().Equal(snapshots[3]);
        }

        [Test]
        public void Should_merge_snapshots_in_batches_when_they_fit()
        {
            var snapshots = Snapshots(41, 18, 35, 4, 1, 90, 22, 50);

            var batches = batcher.Batch(snapshots).ToArray();

            batches.Should().HaveCount(3);
            batches[0].Should().Equal(snapshots[5]);
            batches[1].Should().Equal(snapshots[7], snapshots[0]);
            batches[2].Should().Equal(snapshots[2], snapshots[6], snapshots[1], snapshots[3], snapshots[4]);
        }

        [Test]
        public void Should_always_return_all_of_given_snapshots()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < 100; i++)
            {
                var snapshots = Snapshots(Enumerable.Range(0, random.Next(10)).Select(_ => random.Next(1, MaximumBatchSize + 1)).ToArray());

                var batches = batcher.Batch(snapshots);

                var allReturnedSnapshots = batches.SelectMany(b => b).ToArray();

                // ReSharper disable once CoVariantArrayConversion
                allReturnedSnapshots.Should().BeEquivalentTo(snapshots);
            }
        }

        [Test]
        public void Should_always_return_batches_of_limited_size()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());

            for (var i = 0; i < 100; i++)
            {
                var snapshots = Snapshots(Enumerable.Range(0, random.Next(10)).Select(_ => random.Next(1, MaximumBatchSize + 1)).ToArray());

                var batches = batcher.Batch(snapshots);

                foreach (var batch in batches)
                {
                    var totalSize = batch.Sum(sn => sn.Data.Count);

                    totalSize.Should().BeLessOrEqualTo(MaximumBatchSize);
                }
            }
        }

        private static BufferSnapshot[] Snapshots(params int[] sizes)
            => sizes.Select(Snapshot).ToArray();

        private static BufferSnapshot Snapshot(int size)
        {
            var source = Substitute.For<IBuffer>();
            var data = new byte[size];

            return new BufferSnapshot(source, new BufferState(data.Length, 10), data);
        }
    }
}