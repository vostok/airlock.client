using System;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Tests.Sink.Buffers
{
    internal class MemoryManger_Tests
    {
        private const long MaxSize = 100;

        private IMemoryManager underlyingMemoryManager;
        private MemoryManager memoryManager;

        [SetUp]
        public void Setup()
        {
            underlyingMemoryManager = Substitute.For<IMemoryManager>();
            underlyingMemoryManager.TryReserveBytes(0).ReturnsForAnyArgs(true);

            memoryManager = new MemoryManager(MaxSize, underlyingMemoryManager);
        }

        [Test]
        public void TryReserveBytes_should_return_true_when_memory_is_available()
        {
            for (var i = 0; i < 10; ++i)
                memoryManager.TryReserveBytes(10).Should().BeTrue();
        }

        [Test]
        public void TryReserveBytes_should_return_false_when_memory_is_exhausted()
        {
            for (var i = 0; i < 10; ++i)
                memoryManager.TryReserveBytes(10);

            memoryManager.TryReserveBytes(1).Should().BeFalse();
        }

        [Test]
        public void ReleaseBytes_should_works_correctly()
        {
            for (var i = 0; i < 10; ++i)
                memoryManager.TryReserveBytes(10);

            memoryManager.TryReserveBytes(1).Should().BeFalse();

            memoryManager.ReleaseBytes(1);

            memoryManager.TryReserveBytes(1).Should().BeTrue();
            memoryManager.TryReserveBytes(1).Should().BeFalse();
        }

        [Test]
        public void Capacity_should_works_correctly()
        {
            for (var i = 0; i < 10; ++i)
            {
                memoryManager.TryReserveBytes(10);
                memoryManager.Capacity.Should().Be((i + 1) * 10);
            }

            memoryManager.TryReserveBytes(1).Should().BeFalse();
            memoryManager.Capacity.Should().Be(MaxSize);

            memoryManager.ReleaseBytes(9);
            memoryManager.Capacity.Should().Be(MaxSize - 9);
        }

        [Test]
        public void LastReserveTicks_should_works_correctly()
        {
            memoryManager.LastReserveTicks.Should().Be(0);

            var before = DateTime.UtcNow.Ticks;
            memoryManager.TryReserveBytes(1);
            var after = DateTime.UtcNow.Ticks;

            memoryManager.LastReserveTicks.Should().BeInRange(before, after);
        }

        [TestCase(true, 10)]
        [TestCase(false, 10)]
        [TestCase(true, 20)]
        [TestCase(false, 20)]
        public void Should_respect_underlying_memory_manager(bool isMemoryAvailable, int amount)
        {
            underlyingMemoryManager.TryReserveBytes(0).ReturnsForAnyArgs(isMemoryAvailable);

            memoryManager.TryReserveBytes(1).Should().Be(isMemoryAvailable);
            underlyingMemoryManager.Received(1).TryReserveBytes(1);
        }

        [Test]
        public void Should_not_call_to_underlying_memory_manager_when_exhausted()
        {
            memoryManager.TryReserveBytes(MaxSize).Should().BeTrue();

            underlyingMemoryManager.ClearReceivedCalls();

            memoryManager.TryReserveBytes(1).Should().BeFalse();
            underlyingMemoryManager.DidNotReceiveWithAnyArgs().TryReserveBytes(0);
        }

        [Test]
        public void Should_not_increase_memory_usage_when_underlying_memory_manager_is_exhausted()
        {
            underlyingMemoryManager.TryReserveBytes(0).ReturnsForAnyArgs(false);

            memoryManager.TryReserveBytes(MaxSize);

            underlyingMemoryManager.TryReserveBytes(0).ReturnsForAnyArgs(true);

            memoryManager.TryReserveBytes(MaxSize).Should().BeTrue();
        }
    }
}