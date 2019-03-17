using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Sink.Statistics;
using Vostok.Hercules.Client.Sink.Writing;

// ReSharper disable AssignNullToNotNullAttribute

namespace Vostok.Hercules.Client.Tests.Sink.Writing
{
    [TestFixture]
    internal class SignalingWriter_Tests
    {
        private IRecordWriter baseWriter;
        private IStatisticsCollector statistics;
        private SignalingWriter signalingWriter;
        private AsyncManualResetEvent signal;

        [SetUp]
        public void TestSetup()
        {
            baseWriter = Substitute.For<IRecordWriter>();
            statistics = Substitute.For<IStatisticsCollector>();
            signal = new AsyncManualResetEvent(false);
            signalingWriter = new SignalingWriter(baseWriter, statistics, signal, 100, 0.25, 0.70);
        }

        [TestCase(RecordWriteResult.Success)]
        [TestCase(RecordWriteResult.Exception)]
        [TestCase(RecordWriteResult.OutOfMemory)]
        [TestCase(RecordWriteResult.RecordTooLarge)]
        public void Should_delegate_to_base_writer(RecordWriteResult result)
        {
            SetupResult(result, 15);

            Write(out var size).Should().Be(result);

            size.Should().Be(15);
        }

        [Test]
        public void Should_not_set_signal_when_below_transition_threshold()
        {
            SetupStoredSizes(10, 24);

            Write(out _);

            signal.WaitAsync().IsCompleted.Should().BeFalse();
        }

        [Test]
        public void Should_not_set_signal_when_above_transition_threshold_but_below_constant_threshold()
        {
            SetupStoredSizes(30, 40);

            Write(out _);

            signal.WaitAsync().IsCompleted.Should().BeFalse();
        }

        [Test]
        public void Should_set_signal_when_crossing_transition_threshold()
        {
            SetupStoredSizes(20, 40);

            Write(out _);

            signal.WaitAsync().IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_set_signal_when_above_constant_threshold()
        {
            SetupStoredSizes(71, 72);

            Write(out _);

            signal.WaitAsync().IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_set_signal_on_oom_result_when_there_are_some_stored_records()
        {
            SetupResult(RecordWriteResult.OutOfMemory, 0);

            SetupStoredSizes(1, 1);

            Write(out _);

            signal.WaitAsync().IsCompleted.Should().BeTrue();
        }

        [Test]
        public void Should_not_set_signal_on_oom_result_when_there_are_no_stored_records()
        {
            SetupResult(RecordWriteResult.OutOfMemory, 0);

            SetupStoredSizes(0, 0);

            Write(out _);

            signal.WaitAsync().IsCompleted.Should().BeFalse();
        }

        private RecordWriteResult Write(out int recordSize)
            => signalingWriter.TryWrite(null, null, out recordSize);

        private void SetupResult(RecordWriteResult result, int recordSize)
            => baseWriter
                .TryWrite(null, null, out _)
                .ReturnsForAnyArgs(
                    info =>
                    {
                        info[2] = recordSize;
                        return result;
                    });

        private void SetupStoredSizes(long before, long after)
            => statistics.EstimateStoredSize().Returns(before, after);
    }
}