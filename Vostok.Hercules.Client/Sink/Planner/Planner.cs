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

        public Task WaitForNextSendAsync(StreamSendResult result, CancellationToken cancellationToken)
        {
            switch (result)
            {
                case StreamSendResult.Success:
                case StreamSendResult.NothingToSend:
                    successiveFailures = 0;
                    break;
                case StreamSendResult.Failure:
                    successiveFailures++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(result));
            }

            return signal
                .WaitAsync(cancellationToken)
                .WaitAsync(GetDelayToNextOccurence());
        }

        private TimeSpan GetDelayToNextOccurence() =>
            Delays.ExponentialWithJitter(sendPeriodCap, sendPeriod, successiveFailures, maxJitterFraction);
    }
}