using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Sink.Sender;

namespace Vostok.Hercules.Client.Sink.Planner
{
    internal class Planner : IPlanner
    {
        private static readonly AsyncManualResetEvent NeverSignaled
            = new AsyncManualResetEvent(false);

        private readonly AsyncManualResetEvent signal;
        private readonly TimeSpan sendPeriod;
        private readonly TimeSpan sendPeriodCap;
        private readonly double maxJitterFraction;

        private volatile int successiveFailures;

        public Planner(AsyncManualResetEvent signal, TimeSpan sendPeriod, TimeSpan sendPeriodCap, double maxJitterFraction)
        {
            this.signal = signal;
            this.sendPeriod = sendPeriod;
            this.sendPeriodCap = sendPeriodCap;
            this.maxJitterFraction = maxJitterFraction;
        }

        public Task WaitForNextSendAsync(StreamSendResult lastResult, CancellationToken cancellationToken)
        {
            if (lastResult.HasTransientFailures)
            {
                Interlocked.Increment(ref successiveFailures);
            }
            else
            {
                Interlocked.Exchange(ref successiveFailures, 0);
            }

            var periodicDelay = Delays.ExponentialWithJitter(sendPeriodCap, sendPeriod, successiveFailures, maxJitterFraction) - lastResult.Elapsed;
            if (periodicDelay <= TimeSpan.Zero)
                return Task.CompletedTask;

            var signalToUse = successiveFailures == 0 ? signal : NeverSignaled;

            return signalToUse
                .WaitAsync(cancellationToken)
                .WaitAsync(periodicDelay);
        }
    }
}