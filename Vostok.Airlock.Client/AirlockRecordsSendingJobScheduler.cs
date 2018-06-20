using System;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordsSendingJobScheduler : IAirlockRecordsSendingJobScheduler
    {
        private readonly IMemoryManager memoryManager;
        private readonly TimeSpan requestSendPeriod;
        private readonly TimeSpan requestSendPeriodCap;

        public AirlockRecordsSendingJobScheduler(IMemoryManager memoryManager, TimeSpan requestSendPeriod, TimeSpan requestSendPeriodCap)
        {
            this.memoryManager = memoryManager;
            this.requestSendPeriod = requestSendPeriod;
            this.requestSendPeriodCap = requestSendPeriodCap;
        }

        public ISchedule GetDelayToNextOccurrence(AirlockRecordsSendingJobState jobState)
        {
            if (jobState.IsSuccess && memoryManager.IsConsumptionAchievedThreshold(50))
            {
                return new Schedule(TimeSpan.Zero);
            }

            var sendPeriod = Delays.Exponential(requestSendPeriodCap, requestSendPeriod, jobState.Attempt).WithEqualJitter().Value;

            var delayToNextOccurrence = jobState.IsSuccess ? sendPeriod - jobState.Elapsed : sendPeriod;

            return new Schedule(delayToNextOccurrence);
        }
    }
}