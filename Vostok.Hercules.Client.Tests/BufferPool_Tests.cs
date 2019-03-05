using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink;

namespace Vostok.Hercules.Client.Tests
{
    internal class BufferPool_Tests
    {
        private IMemoryManager memoryManager;
        private readonly int initialBufferSize = 100;
        private readonly int maxRecordSize = 300;
        private readonly int maxBufferSize = 1000;
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
            buffer.MakeSnapshot().Buffer.Length.Should().Be(initialBufferSize);
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
        public void MakeSnapshot_should_return_acquired_buffer()
        {
            bufferPool.TryAcquire(out var buffer);
            var writer = buffer.BeginRecord();
            writer.Write(0);
            buffer.Commit(sizeof(int));

            var snapshot = bufferPool.MakeSnapshot();

            snapshot.Should().BeEquivalentTo(buffer);
        }

        [Test]
        public void MakeSnapshot_should_return_released_buffer_with_data()
        {
            bufferPool.TryAcquire(out var buffer);
            var writer = buffer.BeginRecord();
            writer.Write(0);
            buffer.Commit(sizeof(int));
            bufferPool.Release(buffer);

            var snapshot = bufferPool.MakeSnapshot();

            snapshot.Should().BeEquivalentTo(buffer);
        }

        [Test]
        public void MakeSnapshot_should_return_null_when_buffers_is_empty()
        {
            bufferPool.TryAcquire(out _);
            bufferPool.TryAcquire(out _);
            var snapshot = bufferPool.MakeSnapshot();

            snapshot.Should().BeNull();
        }

        [Test]
        public void MakeSnapshot_should_return_only_buffers_with_data()
        {
            bufferPool.TryAcquire(out var buffer);
            bufferPool.TryAcquire(out _);
            var writer = buffer.BeginRecord();
            writer.Write(0);
            buffer.Commit(sizeof(int));

            var snapshot = bufferPool.MakeSnapshot();

            snapshot.Should().BeEquivalentTo(buffer);
        }
    }
}