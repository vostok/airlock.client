using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordsSendingJobSchedule : IAirlockRecordsSendingJobSchedule
    {
        private readonly IMemoryManager memoryManager;
        private readonly int requestSendPeriodMs;
        private readonly int requestSendPeriodCapMs;

        private volatile AirlockRecordsSendingJobState lastJobState = AirlockRecordsSendingJobState.Unknown;

        public AirlockRecordsSendingJobSchedule(IMemoryManager memoryManager, int requestSendPeriodMs, int requestSendPeriodCapMs)
        {
            this.memoryManager = memoryManager;
            this.requestSendPeriodMs = requestSendPeriodMs;
            this.requestSendPeriodCapMs = requestSendPeriodCapMs;
        }

        public async Task WaitNextOccurrenceAsync(CancellationToken cancellationToken = default)
        {
            if (lastJobState.Result && memoryManager.IsConsumptionAchievedThreshold(50))
            {
                return;
            }

            var sendPeriodMs = Delays.Expotential(requestSendPeriodCapMs, requestSendPeriodMs, lastJobState.Attempt).WithEqualJitter().DelayMs;

            var delayToNextOccurrence = GetDelayToNextOccurrence(TimeSpan.FromMilliseconds(sendPeriodMs));

            if (delayToNextOccurrence > TimeSpan.Zero)
            {
                await Task.Delay(delayToNextOccurrence, cancellationToken).ConfigureAwait(false);
            }
        }

        public void SetLastJobRunningState(AirlockRecordsSendingJobState jobState)
        {
            lastJobState = jobState;
        }

        private TimeSpan GetDelayToNextOccurrence(TimeSpan sendPeriod)
        {
            return lastJobState.Result ? sendPeriod - lastJobState.Elapsed : sendPeriod;
        }
    }
}