using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Sink.Analyzer;
using Vostok.Hercules.Client.Sink.Sender;

namespace Vostok.Hercules.Client.Sink.Planning
{
    internal class Planner : IPlanner
    {
        private static readonly AsyncManualResetEvent NeverSignaled
            = new AsyncManualResetEvent(false);

        private readonly IStatusAnalyzer statusAnalyzer;
        private readonly AsyncManualResetEvent signal;
        private readonly TimeSpan sendPeriod;
        private readonly TimeSpan sendPeriodCap;
        private readonly double maxJitterFraction;

        private volatile int backoffDepth;

        public Planner(
            [NotNull] IStatusAnalyzer statusAnalyzer,
            [NotNull] AsyncManualResetEvent signal,
            TimeSpan sendPeriod,
            TimeSpan sendPeriodCap,
            double maxJitterFraction)
        {
            this.statusAnalyzer = statusAnalyzer;
            this.signal = signal;
            this.sendPeriod = sendPeriod;
            this.sendPeriodCap = sendPeriodCap;
            this.maxJitterFraction = maxJitterFraction;
        }

        public async Task WaitForNextSendAsync(StreamSendResult lastResult, CancellationToken cancellationToken)
        {
            if (statusAnalyzer.ShouldIncreaseSendPeriod(lastResult.Status))
            {
                Interlocked.Increment(ref backoffDepth);
            }
            else
            {
                Interlocked.Exchange(ref backoffDepth, 0);
            }

            var periodicDelay = Delays.ExponentialWithJitter(sendPeriodCap, sendPeriod, backoffDepth, maxJitterFraction) - lastResult.Elapsed;
            if (periodicDelay <= TimeSpan.Zero)
                return;

            var signalToUse = backoffDepth == 0 ? signal : NeverSignaled;

            await signalToUse
                .WaitAsync(cancellationToken, periodicDelay)
                .ConfigureAwait(false);

            signalToUse.Reset();
        }
    }
}