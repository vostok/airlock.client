using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink;

namespace Vostok.Hercules.Client.Tests
{
    internal class BufferPool_Tests
    {
        private IMemoryManager memoryManager;
        private int initialBufferSize = 100;
        private int maxRecordSize = 300;
        private int maxBufferSize = 1000;
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
    }
}