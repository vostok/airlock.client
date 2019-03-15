using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Tests.Sink
{
    internal class BufferPool_Tests
    {
        private const int InitialBufferSize = 100;
        private const int MaxRecordSize = 300;
        private const int MaxBufferSize = 1000;

        private IMemoryManager memoryManager;
        private BufferPool bufferPool;

        [SetUp]
        public void Setup()
        {
            memoryManager = Substitute.For<IMemoryManager>();
            bufferPool = new BufferPool(memoryManager, InitialBufferSize, MaxRecordSize, MaxBufferSize);

            memoryManager.TryReserveBytes(0).ReturnsForAnyArgs(true);
        }

        [Test]
        public void Should_respect_initialBufferSize_setting()
        {
            bufferPool.TryAcquire(out var buffer).Should().BeTrue();
            buffer.TryMakeSnapshot().Data.Array.Length.Should().Be(InitialBufferSize);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Should_control_memory_usage_with_memoryManager(bool canAllocate)
        {
            memoryManager.TryReserveBytes(0).ReturnsForAnyArgs(canAllocate);
            bufferPool.TryAcquire(out _).Should().Be(canAllocate);
            memoryManager.Received(1).TryReserveBytes(InitialBufferSize);
        }

        [Test]
        public void Should_reuse_released_buffer()
        {
            bufferPool.TryAcquire(out var first);
            bufferPool.Release(first);
            bufferPool.TryAcquire(out var second);

            second.Should().BeSameAs(first);
        }

        [Test]
        public void Should_reuse_released_buffers_in_FIFO_order()
        {
            bufferPool.TryAcquire(out var first);
            bufferPool.TryAcquire(out var second);
            bufferPool.Release(first);
            bufferPool.Release(second);
            bufferPool.TryAcquire(out var third);
            bufferPool.TryAcquire(out var fourth);

            third.Should().BeSameAs(first);
            fourth.Should().BeSameAs(second);
        }

        [Test]
        public void Should_create_new_buffers_when_empty()
        {
            bufferPool.TryAcquire(out var first);
            bufferPool.TryAcquire(out var second);

            second.Should().NotBeSameAs(first);
        }

        [Test]
        public void Enumerator_should_return_acquired_buffer()
        {
            bufferPool.TryAcquire(out var buffer);
            buffer.Write(0);
            buffer.Commit(sizeof(int));

            var snapshot = bufferPool.ToArray();

            snapshot.Should().BeEquivalentTo(buffer);
        }

        [Test]
        public void Enumerator_should_return_released_buffer_with_data()
        {
            bufferPool.TryAcquire(out var buffer);
            buffer.Write(0);
            buffer.Commit(sizeof(int));
            bufferPool.Release(buffer);

            var snapshot = bufferPool.ToArray();

            snapshot.Should().BeEquivalentTo(buffer);
        }

        [Test]
        public void Should_lock_returned_cached_buffers_for_writes()
        {
            bufferPool.TryAcquire(out var buffer);

            buffer.Should().BeOfType<Buffer>().Which.TryLock().Should().BeFalse();
        }

        [Test]
        public void Should_lock_returned_allocated_buffers_for_writes()
        {
            bufferPool.TryAcquire(out var buffer);
            bufferPool.TryAcquire(out buffer);

            buffer.Should().BeOfType<Buffer>().Which.TryLock().Should().BeFalse();
        }

        [Test]
        public void Should_unlock_released_buffers()
        {
            bufferPool.TryAcquire(out var buffer);

            bufferPool.Release(buffer);

            buffer.Should().BeOfType<Buffer>().Which.TryLock().Should().BeTrue();
        }
    }
}