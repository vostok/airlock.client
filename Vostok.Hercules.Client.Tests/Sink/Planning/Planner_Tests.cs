using System;
using System.Diagnostics;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Time;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Sink.Planning;
using Vostok.Hercules.Client.Sink.Sender;

// ReSharper disable ImpureMethodCallOnReadonlyValueField

namespace Vostok.Hercules.Client.Tests.Sink.Planning
{
    [TestFixture]
    internal class Planner_Tests
    {
        private Planner planner;
        private AsyncManualResetEvent signal;
        private CancellationTokenSource cancellation;

        private static readonly TimeSpan SendPeriod = 100.Milliseconds();
        private static readonly TimeSpan SendPeriodCap = 850.Milliseconds();

        [SetUp]
        public void TestSetup()
        {
            signal = new AsyncManualResetEvent(false);
            cancellation = new CancellationTokenSource();
            planner = new Planner(signal, SendPeriod, SendPeriodCap, 0d);
        }

        [Test]
        public void WaitForNextSendAsync_should_complete_immediately_when_cancellation_token_is_set()
        {
            cancellation.Cancel();

            MeasureWaitDelay(GateResponseClass.Success).Should().Be(TimeSpan.Zero);
            MeasureWaitDelay(GateResponseClass.TransientFailure).Should().Be(TimeSpan.Zero);
        }

        [Test]
        public void WaitForNextSendAsync_should_complete_immediately_when_signal_is_set_and_last_result_was_successful()
        {
            signal.Set();

            MeasureWaitDelay(GateResponseClass.Success).Should().Be(TimeSpan.Zero);
        }

        [Test]
        public void WaitForNextSendAsync_should_complete_immediately_when_signal_is_set_and_last_result_was_a_definitive_failure()
        {
            signal.Set();

            MeasureWaitDelay(GateResponseClass.DefinitiveFailure).Should().Be(TimeSpan.Zero);
        }

        [Test]
        public void WaitForNextSendAsync_should_not_complete_immediately_when_signal_is_set_but_last_result_was_a_transient_failure()
        {
            signal.Set();

            MeasureWaitDelay(GateResponseClass.TransientFailure).Should().BeGreaterThan(TimeSpan.Zero);
        }

        [Test]
        public void WaitForNextSendAsync_should_wait_for_send_period_when_nothing_special_happens()
        {
            for (var i = 0; i < 3; i++)
            {
                MeasureWaitDelay(GateResponseClass.Success).Should().BeCloseTo(SendPeriod, SendPeriod);
            }
        }

        [Test]
        public void WaitForNextSendAsync_should_back_off_exponentially_on_transient_failures()
        {
            for (var i = 1; i <= 3; i++)
            {
                MeasureWaitDelay(GateResponseClass.TransientFailure).Should().BeCloseTo(SendPeriod.Multiply(Math.Pow(2, i)), SendPeriod);
            }
        }

        [Test]
        public void WaitForNextSendAsync_should_return_to_base_send_period_after_a_successful_send()
        {
            for (var i = 1; i <= 3; i++)
                MeasureWaitDelay(GateResponseClass.TransientFailure);

            MeasureWaitDelay(GateResponseClass.Success).Should().BeCloseTo(SendPeriod, SendPeriod);
        }

        [Test]
        public void WaitForNextSendAsync_should_consider_the_signal_agaion_after_a_successful_send()
        {
            for (var i = 1; i <= 3; i++)
                MeasureWaitDelay(GateResponseClass.TransientFailure);

            signal.Set();

            MeasureWaitDelay(GateResponseClass.Success).Should().Be(TimeSpan.Zero);
        }

        private TimeSpan MeasureWaitDelay(GateResponseClass lastResponse, TimeSpan? lastLatency = null)
        {
            var sendResult = new StreamSendResult(new[] { lastResponse }, lastLatency ?? TimeSpan.Zero);

            var waitTask = planner.WaitForNextSendAsync(sendResult, cancellation.Token);
            if (waitTask.IsCompleted)
                return TimeSpan.Zero;

            var watch = Stopwatch.StartNew();

            waitTask.GetAwaiter().GetResult();

            return watch.Elapsed;
        }
    }
}