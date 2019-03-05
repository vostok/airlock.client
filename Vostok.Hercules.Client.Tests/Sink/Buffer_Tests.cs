using FluentAssertions;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Tests.Sink
{
    internal class Buffer_Tests
    {
        [Test]
        public void Position_should_be_zero_after_creation()
        {
            var memManager = new MemoryManager(sizeof(int) * 4);
            var buffer = new Buffer(0, memManager);
            buffer.Position.Should().Be(0);
        }

        [Test]
        public void Should_set_overflow_flag_on_overflow()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, memManager);

            for (var i = 0; i < 5; i++)
                buffer.Write(0);

            buffer.IsOverflowed.Should().BeTrue();
        }

        [Test]
        public void CollectGarbage_should_reset_buffer_when_all_records_are_garbage()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, memManager);

            buffer.Write(0);
            buffer.Commit(sizeof(int));
            buffer.RequestGarbageCollection(buffer.GetState());
            buffer.CollectGarbage();

            buffer.Position.Should().Be(0);
            buffer.GetState().Should().Be(new BufferState());
        }

        [Test]
        public void CollectGarbage_should_remove_only_garbage_records()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, memManager);

            buffer.Write(0);
            buffer.Write(0);
            buffer.Write(0);
            buffer.Commit(sizeof(int));
            buffer.Commit(sizeof(int));
            buffer.Commit(sizeof(int));
            buffer.RequestGarbageCollection(new BufferState(sizeof(int), 1));
            buffer.CollectGarbage();

            buffer.Position.Should().Be(2 * sizeof(int));
            buffer.GetState().Should().Be(new BufferState(2 * sizeof(int), 2));
        }
    }
}