using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Time;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Sink.Analyzer;
using Vostok.Hercules.Client.Sink.Planning;
using Vostok.Hercules.Client.Sink.Sender;

// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace Vostok.Hercules.Client.Tests.Sink.Planning
{
    [TestFixture]
    internal class Planner_Tests
    {
        private IStatusAnalyzer statusAnalyzer;
        private AsyncManualResetEvent signal;
        private CancellationTokenSource cancellation;
        private Planner planner;

        private static readonly TimeSpan SendPeriod = 100.Milliseconds();
        private static readonly TimeSpan SendPeriodCap = 850.Milliseconds();

        [SetUp]
        public void TestSetup()
        {
            statusAnalyzer = Substitute.For<IStatusAnalyzer>();
            statusAnalyzer.ShouldIncreaseSendPeriod(Arg.Any<HerculesStatus>()).Returns(false);
            statusAnalyzer.ShouldIncreaseSendPeriod(HerculesStatus.NetworkError).Returns(true);

            signal = new AsyncManualResetEvent(false);
            cancellation = new CancellationTokenSource();

            planner = new Planner(statusAnalyzer, signal, SendPeriod, SendPeriodCap, 0d);
        }

        [Test]
        public void WaitForNextSendAsync_should_complete_immediately_when_cancellation_token_is_set()
        {
            cancellation.Cancel();

            MeasureWaitDelay(HerculesStatus.Success).Should().Be(TimeSpan.Zero);
            MeasureWaitDelay(HerculesStatus.NetworkError).Should().Be(TimeSpan.Zero);
        }

        [Test]
        public void WaitForNextSendAsync_should_complete_immediately_when_signal_is_set_and_there_is_no_backoff()
        {
            signal.Set();

            MeasureWaitDelay(HerculesStatus.Success).Should().Be(TimeSpan.Zero);
        }

        [Test]
        public void WaitForNextSendAsync_should_not_complete_immediately_when_signal_is_set_but_there_is_backoff()
        {
            signal.Set();

            MeasureWaitDelay(HerculesStatus.NetworkError).Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Test]
        public void WaitForNextSendAsync_should_complete_immediately_when_computed_period_is_already_due_because_of_send_latency()
        {
            MeasureWaitDelay(HerculesStatus.ServerError, SendPeriodCap).Should().Be(TimeSpan.Zero);
        }

        [Test]
        public void WaitForNextSendAsync_should_wait_for_send_period_when_nothing_special_happens()
        {
            for (var i = 0; i < 3; i++)
            {
                MeasureWaitDelay(HerculesStatus.Success).Should().BeCloseTo(SendPeriod, SendPeriod);
            }
        }

        [Test]
        public void WaitForNextSendAsync_should_back_off_exponentially_on_transient_failures()
        {
            for (var i = 1; i <= 3; i++)
            {
                MeasureWaitDelay(HerculesStatus.NetworkError).Should().BeCloseTo(SendPeriod.Multiply(Math.Pow(2, i)), SendPeriod);
            }
        }

        [Test]
        public void WaitForNextSendAsync_should_return_to_base_send_period_after_a_successful_send()
        {
            for (var i = 1; i <= 3; i++)
                MeasureWaitDelay(HerculesStatus.NetworkError);

            MeasureWaitDelay(HerculesStatus.Success).Should().BeCloseTo(SendPeriod, SendPeriod);
        }

        [Test]
        public void WaitForNextSendAsync_should_consider_the_signal_again_after_a_successful_send()
        {
            for (var i = 1; i <= 3; i++)
                MeasureWaitDelay(HerculesStatus.NetworkError);

            signal.Set();

            MeasureWaitDelay(HerculesStatus.Success).Should().Be(TimeSpan.Zero);
        }

        private TimeSpan MeasureWaitDelay(HerculesStatus lastStatus, TimeSpan? lastLatency = null)
        {
            var sendResult = new StreamSendResult(lastStatus, lastLatency ?? TimeSpan.Zero);

            var waitTask = planner.WaitForNextSendAsync(sendResult, cancellation.Token);
            if (waitTask.IsCompleted)
                return TimeSpan.Zero;

            var watch = Stopwatch.StartNew();

            waitTask.GetAwaiter().GetResult();

            return watch.Elapsed;
        }
    }
}