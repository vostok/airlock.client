using FluentAssertions;
using NUnit.Framework;

namespace Vostok.Hercules.Client.Tests
{
    public class BufferTests
    {
        [Test]
        public void Should_have_reserved_space_for_size()
        {
            var memManager = new MemoryManager(sizeof(int)*4);
            var buffer = new Buffer(0, memManager);
            buffer.Position.Should().Be(sizeof(int));
        }
        
        [Test]
        public void Should_set_overflow_flag_on_overflow()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, memManager);
            
            for (var i = 0; i < 4; i++)
                buffer.Write(0);

            buffer.IsOverflowed.Should().BeTrue();
        }
        
        [Test]
        public void TryLock_should_return_false_if_buffer_is_already_locked()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, memManager);

            buffer.TryLock().Should().BeTrue();
            buffer.TryLock().Should().BeFalse();
        }
        
        [Test]
        public void Unlock_should_unlock_buffer()
        {
            var memManager = new MemoryManager(0);
            var buffer = new Buffer(16, memManager);

            buffer.TryLock().Should().BeTrue();
            buffer.Unlock();
            buffer.TryLock().Should().BeTrue();
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

            buffer.Position.Should().Be(Buffer.InitialPosition);
            buffer.GetState().Should().Be(new BufferState(Buffer.InitialPosition, 0));
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
            buffer.RequestGarbageCollection(new BufferState(Buffer.InitialPosition + sizeof(int), 1));
            buffer.CollectGarbage();

            buffer.Position.Should().Be(Buffer.InitialPosition + 2 * sizeof(int));
            buffer.GetState().Should().Be(new BufferState(Buffer.InitialPosition + 2 * sizeof(int), 2));
        }
    }
}