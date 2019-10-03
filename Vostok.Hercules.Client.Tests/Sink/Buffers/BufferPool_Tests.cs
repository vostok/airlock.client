using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Buffers;
using Buffer = Vostok.Hercules.Client.Sink.Buffers.Buffer;

namespace Vostok.Hercules.Client.Tests.Sink.Buffers
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

            buffer.TryMakeSnapshot()?.Data.Array?.Length.Should().Be(InitialBufferSize);
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
            buffer.CommitRecord(sizeof(int));

            bufferPool.Should().Equal(buffer);
        }

        [Test]
        public void Enumerator_should_return_released_buffer_with_data()
        {
            bufferPool.TryAcquire(out var buffer);
            buffer.Write(0);
            buffer.CommitRecord(sizeof(int));
            bufferPool.Release(buffer);

            bufferPool.Should().Equal(buffer);
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

        [Test]
        public void Acquire_should_collect_garbage()
        {
            bufferPool.TryAcquire(out var buffer);

            buffer.Write(Guid.Empty);
            buffer.CommitRecord(16);
            buffer.ReportGarbage(new BufferState(16, 1));

            bufferPool.Release(buffer);

            bufferPool.TryAcquire(out var secondBuffer).Should().BeTrue();

            secondBuffer.Should().BeSameAs(buffer);

            ((Buffer)buffer).Garbage.IsEmpty.Should().BeTrue();
        }

        [Test]
        public void Acquire_should_not_return_locked_buffers()
        {
            bufferPool.TryAcquire(out var buffer);

            bufferPool.Release(buffer);

            ((Buffer)buffer).TryLock();

            bufferPool.TryAcquire(out var secondBuffer).Should().BeTrue();

            secondBuffer.Should().NotBeSameAs(buffer);
        }

        [Test]
        public void Acquire_should_not_return_buffers_with_not_enough_space_for_max_record_size()
        {
            bufferPool.TryAcquire(out var buffer);

            buffer.WriteWithoutLength(new byte[MaxBufferSize - 1]);
            buffer.CommitRecord(MaxBufferSize - 1);

            bufferPool.Release(buffer);

            bufferPool.TryAcquire(out var secondBuffer).Should().BeTrue();

            secondBuffer.Should().NotBeSameAs(buffer);
        }

        [Test]
        public void Free_should_remove_buffer_and_release_memory()
        {
            bufferPool.TryAcquire(out var first);
            bufferPool.TryAcquire(out var second);

            bufferPool.Free(first);
            bufferPool.Release(second);

            bufferPool.Should().BeEquivalentTo(second);
            memoryManager.Received(2).TryReserveBytes(InitialBufferSize);
            memoryManager.Received().ReleaseBytes(InitialBufferSize);
        }
    }
}