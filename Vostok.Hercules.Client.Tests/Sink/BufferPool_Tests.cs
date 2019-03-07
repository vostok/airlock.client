using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Tests.Sink
{
    internal class BufferPool_Tests
    {
        private readonly int initialBufferSize = 100;
        private readonly int maxRecordSize = 300;
        private readonly int maxBufferSize = 1000;
        private IMemoryManager memoryManager;
        private BufferPool bufferPool;

        [SetUp]
        public void Setup()
        {
            memoryManager = Substitute.For<IMemoryManager>();
            bufferPool = new BufferPool(memoryManager, initialBufferSize, maxRecordSize, maxBufferSize);

            memoryManager.TryReserveBytes(0).ReturnsForAnyArgs(true);
        }

        [Test]
        public void Should_respect_initialBufferSize_setting()
        {
            bufferPool.TryAcquire(out var buffer).Should().BeTrue();
            buffer.TryMakeSnapshot().Buffer.Length.Should().Be(initialBufferSize);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Should_control_memory_usage_with_memoryManager(bool canAllocate)
        {
            memoryManager.TryReserveBytes(0).ReturnsForAnyArgs(canAllocate);
            bufferPool.TryAcquire(out _).Should().Be(canAllocate);
            memoryManager.Received(1).TryReserveBytes(initialBufferSize);
        }

        [Test]
        public void Should_reuse_released_buffer()
        {
            bufferPool.TryAcquire(out var first);
            bufferPool.Release(first);
            bufferPool.TryAcquire(out var second);

            second.Should().Be(first);
        }

        [Test]
        public void Should_reuse_released_buffer_in_FIFO_order()
        {
            bufferPool.TryAcquire(out var first);
            bufferPool.TryAcquire(out var second);
            bufferPool.Release(first);
            bufferPool.Release(second);
            bufferPool.TryAcquire(out var third);
            bufferPool.TryAcquire(out var fourth);

            third.Should().Be(first);
            fourth.Should().Be(second);
        }

        [Test]
        public void Should_create_new_buffers_when_empty()
        {
            bufferPool.TryAcquire(out var first);
            bufferPool.TryAcquire(out var second);
            second.Should().NotBe(first);
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
    }
}