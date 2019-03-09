using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Threading;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal class SchedulingStreamSender : ISchedulingStreamSender
    {
        private readonly AsyncManualResetEvent sendImmediately = new AsyncManualResetEvent(false);
        private readonly IStreamSender sender;
        private readonly TimeSpan sendPeriod;
        private readonly TimeSpan sendPeriodCap;

        private int unsuccessfulAttempts;

        public SchedulingStreamSender(IStreamSender sender, TimeSpan sendPeriod, TimeSpan sendPeriodCap)
        {
            this.sender = sender;
            this.sendPeriod = sendPeriod;
            this.sendPeriodCap = sendPeriodCap;

            Signal = GetDelayToNextOccurence();
        }

        public Task Signal { get; private set; }

        public async Task<SendResult> SendAsync(TimeSpan timeout, CancellationToken cancellationToken)
        {
            sendImmediately.Reset();

            var sendResult = await sender.SendAsync(timeout, cancellationToken).ConfigureAwait(false);

            switch (sendResult)
            {
                case SendResult.Success:
                case SendResult.NothingToSend:
                    unsuccessfulAttempts = 0;
                    break;
                case SendResult.Failure:
                    unsuccessfulAttempts++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sendResult));
            }

            Signal = Task.WhenAny(GetDelayToNextOccurence(), sendImmediately);

            return sendResult;
        }

        public void Wakeup() => sendImmediately.Set();

        private Task GetDelayToNextOccurence()
        {
            var delayToNextOccurence = Delays.ExponentialWithJitter(sendPeriodCap, sendPeriod, unsuccessfulAttempts);
            return Task.Delay(delayToNextOccurence);
        }
    }
}