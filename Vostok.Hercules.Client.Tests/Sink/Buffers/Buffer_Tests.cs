using FluentAssertions;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Tests.Sink.Buffers
{
    internal class Buffer_Tests
    {
        [Test]
        public void Position_should_be_zero_after_creation()
        {
            var memManager = new MemoryManager(sizeof(int) * 4);
            var buffer = new Buffer(0, 1000, memManager);
            buffer.Position.Should().Be(0);
        }

        [Test]
        public void Should_set_overflow_flag_on_overflow()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, 1000, memManager);

            for (var i = 0; i < 5; i++)
                buffer.Write(0);

            buffer.IsOverflowed.Should().BeTrue();
        }

        [Test]
        public void CollectGarbage_should_reset_buffer_when_all_records_are_garbage()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, 1000, memManager);

            buffer.Write(0);
            buffer.CommitRecord(sizeof(int));
            buffer.ReportGarbage(buffer.Committed);
            buffer.TryCollectGarbage();

            buffer.Position.Should().Be(0);
            buffer.Committed.Should().Be(new BufferState());
        }

        [Test]
        public void CollectGarbage_should_remove_only_garbage_records()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, 1000, memManager);

            buffer.Write(0);
            buffer.Write(0);
            buffer.Write(0);
            buffer.CommitRecord(sizeof(int));
            buffer.CommitRecord(sizeof(int));
            buffer.CommitRecord(sizeof(int));
            buffer.ReportGarbage(new BufferState(sizeof(int), 1));
            buffer.TryCollectGarbage();

            buffer.Position.Should().Be(2 * sizeof(int));
            buffer.Committed.Should().Be(new BufferState(2 * sizeof(int), 2));
        }

        [Test]
        public void TryLock_should_return_false_if_buffer_is_already_locked()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, 1000, memManager);

            buffer.TryLock().Should().BeTrue();
            buffer.TryLock().Should().BeFalse();
        }

        [Test]
        public void Unlock_should_unlock_buffer()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, 1000, memManager);

            buffer.TryLock().Should().BeTrue();
            buffer.Unlock();
            buffer.TryLock().Should().BeTrue();
        }

        [Test]
        public void Should_not_grow_larger_than_maxSize()
        {
            var maxSize = 20;

            var memManager = new MemoryManager(1000);
            var buffer = new Buffer(16, 20, memManager);

            buffer.Write(0L);
            buffer.Write(0L);
            buffer.Write((byte)0);

            buffer.Capacity.Should().Be(maxSize);
        }

        [Test]
        public void Should_grow_twice_as_large_by_default()
        {
            var initialSize = 16;
            
            var memManager = new MemoryManager(1000);
            var buffer = new Buffer(initialSize, 1000, memManager);

            buffer.Write(0L);
            buffer.Write(0L);
            buffer.Write((byte)0);

            buffer.Capacity.Should().Be(2 * initialSize);
        }
    }
}