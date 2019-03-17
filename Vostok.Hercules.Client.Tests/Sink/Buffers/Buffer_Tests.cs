using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Hercules.Client.Sink.Buffers;
using Buffer = Vostok.Hercules.Client.Sink.Buffers.Buffer;
// ReSharper disable PossibleNullReferenceException

namespace Vostok.Hercules.Client.Tests.Sink.Buffers
{
    internal class Buffer_Tests
    {
        private IMemoryManager manager;
        private Buffer buffer;

        private const int InitialSize = 16;
        private const int MaximumSize = 100;

        [SetUp]
        public void TestSetup()
        {
            manager = Substitute.For<IMemoryManager>();
            manager.TryReserveBytes(Arg.Any<long>()).Returns(true);

            buffer = new Buffer(InitialSize, MaximumSize, manager);
        }

        [Test]
        public void Should_initially_have_zero_committed_state()
        {
            buffer.Committed.Length.Should().Be(0);
            buffer.Committed.RecordsCount.Should().Be(0);
        }

        [Test]
        public void Should_initially_have_zero_garbage_state()
        {
            buffer.Garbage.Length.Should().Be(0);
            buffer.Garbage.RecordsCount.Should().Be(0);
        }

        [Test]
        public void Should_initially_have_zero_useful_data_size()
        {
            buffer.UsefulDataSize.Should().Be(0L);
        }

        [Test]
        public void CommitRecord_should_fail_on_negative_size()
        {
            Action action = () => buffer.CommitRecord(-1);

            action.Should().Throw<ArgumentOutOfRangeException>().Which.ShouldBePrinted();
        }

        [Test]
        public void CommitRecord_should_fail_on_zero_size()
        {
            Action action = () => buffer.CommitRecord(0);

            action.Should().Throw<ArgumentOutOfRangeException>().Which.ShouldBePrinted();
        }

        [Test]
        public void CommitRecord_should_fail_when_attempting_to_commit_past_current_physical_length()
        {
            buffer.Write(Guid.NewGuid());

            Action action = () => buffer.CommitRecord(17);

            action.Should().Throw<InvalidOperationException>().Which.ShouldBePrinted();
        }

        [Test]
        public void CommitRecord_should_increase_committed_length_by_given_amount()
        {
            for (var i = 1; i <= 3; i++)
            {
                buffer.Write(Guid.NewGuid());

                buffer.CommitRecord(16);

                buffer.Committed.Length.Should().Be(16 * i);
            }
        }

        [Test]
        public void CommitRecord_should_increase_committed_count_by_one()
        {
            for (var i = 1; i <= 3; i++)
            {
                buffer.Write(Guid.NewGuid());

                buffer.CommitRecord(16);

                buffer.Committed.RecordsCount.Should().Be(i);
            }
        }

        [Test]
        public void ReportGarbage_should_mark_given_region_as_garbage()
        {
            WriteAndCommit(5);
            WriteAndCommit(5);
            WriteAndCommit(10);

            var garbageRegion = new BufferState(10, 2);

            buffer.ReportGarbage(garbageRegion);

            buffer.Garbage.Should().Be(garbageRegion);
        }

        [Test]
        public void ReportGarbage_should_fail_if_there_is_already_some_garbage()
        {
            WriteAndCommit(5);
            WriteAndCommit(5);
            WriteAndCommit(10);

            buffer.ReportGarbage(new BufferState(5, 1));

            Action action = () => buffer.ReportGarbage(new BufferState(10, 2));

            action.Should().Throw<InvalidOperationException>().Which.ShouldBePrinted();
        }

        [Test]
        public void ReportGarbage_should_fail_if_given_region_size_exceeds_current_commited_size()
        {
            WriteAndCommit(5);
            WriteAndCommit(5);
            WriteAndCommit(10);

            buffer.Write(Guid.NewGuid());

            Action action = () => buffer.ReportGarbage(new BufferState(21, 3));

            action.Should().Throw<InvalidOperationException>().Which.ShouldBePrinted();
        }

        [Test]
        public void ReportGarbage_should_fail_if_given_region_has_more_records_than_committed_region()
        {
            WriteAndCommit(5);
            WriteAndCommit(5);
            WriteAndCommit(10);

            buffer.Write(Guid.NewGuid());

            Action action = () => buffer.ReportGarbage(new BufferState(20, 4));

            action.Should().Throw<InvalidOperationException>().Which.ShouldBePrinted();
        }

        [Test]
        public void UsefulDataSize_should_report_the_difference_between_committed_and_garbage_region_sizes()
        {
            WriteAndCommit(5);
            WriteAndCommit(5);
            WriteAndCommit(11);

            buffer.Write(Guid.NewGuid());

            buffer.ReportGarbage(new BufferState(10, 2));

            buffer.UsefulDataSize.Should().Be(11);
        }

        [Test]
        public void TryLock_should_lock_buffer_and_prevent_other_locks()
        {
            buffer.TryLock().Should().BeTrue();

            buffer.TryLock().Should().BeFalse();
            buffer.TryLock().Should().BeFalse();
        }

        [Test]
        public void Unlock_should_allow_to_lock_buffer_again()
        {
            buffer.TryLock().Should().BeTrue();

            buffer.Unlock();

            buffer.TryLock().Should().BeTrue();
        }

        [Test]
        public void TryCollectGarbage_should_return_true_when_there_is_no_garbage()
        {
            WriteAndCommit(10);
            WriteAndCommit(10);

            buffer.TryCollectGarbage().Should().BeTrue();
        }

        [Test]
        public void TryCollectGarbage_should_return_true_when_there_is_no_garbage_even_if_buffer_is_locked()
        {
            WriteAndCommit(10);
            WriteAndCommit(10);

            buffer.TryLock();

            buffer.TryCollectGarbage().Should().BeTrue();
        }

        [Test]
        public void TryCollectGarbage_should_return_false_when_the_buffer_cannot_be_locked()
        {
            WriteAndCommit(4);
            WriteAndCommit(6);
            WriteAndCommit(10);

            buffer.ReportGarbage(new BufferState(10 ,2));

            buffer.TryLock();

            buffer.TryCollectGarbage().Should().BeFalse();

            buffer.Position.Should().Be(20);
            buffer.Garbage.Length.Should().Be(10);
        }

        [Test]
        public void TryCollectGarbage_should_move_useful_and_uncommitted_data_to_the_start_of_the_buffer()
        {
            var usefulData = Guid.NewGuid().ToByteArray();
            var uncommittedData = Guid.NewGuid().ToByteArray();

            WriteAndCommit(4);
            WriteAndCommit(6);
            WriteAndCommit(usefulData);

            buffer.WriteWithoutLength(uncommittedData);

            buffer.ReportGarbage(new BufferState(10, 2));

            buffer.TryCollectGarbage().Should().BeTrue();

            buffer.Position.Should().Be(usefulData.Length + uncommittedData.Length);
            buffer.CommittedSegment.Should().Equal(usefulData);

            buffer.CommitRecord(uncommittedData.Length);
            buffer.CommittedSegment.Should().Equal(usefulData.Concat(uncommittedData));
        }

        [Test]
        public void TryCollectGarbage_should_substract_garbage_region_from_current_committed_state()
        {
            WriteAndCommit(4);
            WriteAndCommit(6);
            WriteAndCommit(11);

            buffer.ReportGarbage(new BufferState(10, 2));

            buffer.TryCollectGarbage().Should().BeTrue();

            buffer.Committed.Length.Should().Be(11);
            buffer.Committed.RecordsCount.Should().Be(1);
        }

        [Test]
        public void TryCollectGarbage_should_zero_out_current_garbage_region()
        {
            WriteAndCommit(4);
            WriteAndCommit(6);
            WriteAndCommit(11);

            buffer.ReportGarbage(new BufferState(10, 2));

            buffer.TryCollectGarbage().Should().BeTrue();

            buffer.Garbage.IsEmpty.Should().BeTrue();
        }

        [Test]
        public void TryCollectGarbage_should_unlock_the_buffer_after_its_done()
        {
            WriteAndCommit(4);
            WriteAndCommit(6);
            WriteAndCommit(11);

            buffer.ReportGarbage(new BufferState(10, 2));

            buffer.TryCollectGarbage().Should().BeTrue();

            buffer.TryLock().Should().BeTrue();
        }

        [Test]
        public void TryCollectGarbage_should_be_able_to_completely_clear_the_buffer()
        {
            WriteAndCommit(4);
            WriteAndCommit(6);
            WriteAndCommit(16);

            buffer.ReportGarbage(new BufferState(26, 3));

            buffer.TryCollectGarbage().Should().BeTrue();

            buffer.Committed.IsEmpty.Should().BeTrue();
            buffer.CommittedSegment.Should().BeEmpty();
            buffer.Position.Should().Be(0L);
        }

        [Test]
        public void TryMakeSnapshot_should_return_a_snapshot_with_correct_source()
        {
            WriteAndCommit(4);
            WriteAndCommit(6);
            WriteAndCommit(16);

            buffer.TryMakeSnapshot()?.Source.Should().BeSameAs(buffer);
        }

        [Test]
        public void TryMakeSnapshot_should_be_able_to_return_an_empty_snapshot()
        {
            var snapshot = buffer.TryMakeSnapshot();

            snapshot.State.IsEmpty.Should().BeTrue();
            snapshot.Data.Should().BeEmpty();
        }

        [Test]
        public void TryMakeSnapshot_should_a_snapshot_with_committed_data_region()
        {
            var committedData = Guid.NewGuid().ToByteArray();
            var uncommittedData = Guid.NewGuid().ToByteArray();

            WriteAndCommit(committedData);

            buffer.WriteWithoutLength(uncommittedData);

            var snapshot = buffer.TryMakeSnapshot();

            snapshot.State.Should().Be(buffer.Committed);
            snapshot.Data.Should().Equal(committedData);
        }

        [Test]
        public void TryMakeSnapshot_should_collect_garbage_before_returning_a_snapshot()
        {
            var committedData = Guid.NewGuid().ToByteArray();
            var uncommittedData = Guid.NewGuid().ToByteArray();

            WriteAndCommit(5);
            WriteAndCommit(6);
            WriteAndCommit(committedData);
            buffer.WriteWithoutLength(uncommittedData);

            buffer.ReportGarbage(new BufferState(11, 2));

            var snapshot = buffer.TryMakeSnapshot();

            snapshot.State.Should().Be(new BufferState(committedData.Length, 1));
            snapshot.Data.Should().Equal(committedData);
        }

        [Test]
        public void TryMakeSnapshot_should_return_null_when_buffer_is_locked_and_has_garbage()
        {
            var committedData = Guid.NewGuid().ToByteArray();
            var uncommittedData = Guid.NewGuid().ToByteArray();

            WriteAndCommit(5);
            WriteAndCommit(6);
            WriteAndCommit(committedData);
            buffer.WriteWithoutLength(uncommittedData);

            buffer.ReportGarbage(new BufferState(11, 2));
            buffer.TryLock();

            buffer.TryMakeSnapshot().Should().BeNull();
        }

        [Test]
        public void Should_correctly_sustain_commit_and_gc_cycle_with_trailing_uncommited_data()
        {
            var uncommitted = Guid.NewGuid().ToByteArray();

            WriteAndCommit(Guid.NewGuid().ToByteArray());
            WriteAndCommit(Guid.NewGuid().ToByteArray());
            buffer.WriteWithoutLength(uncommitted);

            for (var i = 0; i < 1; i++)
            {
                buffer.Committed.Should().Be(new BufferState(32, 2));
                buffer.Garbage.Should().Be(new BufferState(0, 0));

                buffer.ReportGarbage(buffer.TryMakeSnapshot().State);

                buffer.Committed.Should().Be(new BufferState(32, 2));
                buffer.Garbage.Should().Be(new BufferState(32, 2));

                buffer.CommitRecord(16);

                buffer.Committed.Should().Be(new BufferState(48, 3));
                buffer.Garbage.Should().Be(new BufferState(32, 2));

                buffer.TryMakeSnapshot();

                buffer.Committed.Should().Be(new BufferState(16, 1));
                buffer.Garbage.Should().Be(new BufferState(0, 0));

                buffer.CommittedSegment.Should().Equal(uncommitted);

                WriteAndCommit(Guid.NewGuid().ToByteArray());
                buffer.WriteWithoutLength(uncommitted = Guid.NewGuid().ToByteArray());
            }

        }

        private void WriteAndCommit(int size)
            => WriteAndCommit(new byte[size]);

        private void WriteAndCommit(byte[] data)
        {
            buffer.WriteWithoutLength(data);

            buffer.CommitRecord(data.Length);
        }
    }
}