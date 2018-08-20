using System;
using System.Collections.Generic;
using Vostok.Airlock.Client.Backoff;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordsSendingJobScheduler : IAirlockRecordsSendingJobScheduler
    {
        private readonly IMemoryManager memoryManager;
        private readonly TimeSpan requestSendPeriod;
        private readonly TimeSpan requestSendPeriodCap;

        private readonly Dictionary<string, int> attempts;

        public AirlockRecordsSendingJobScheduler(IMemoryManager memoryManager, TimeSpan requestSendPeriod, TimeSpan requestSendPeriodCap)
        {
            this.memoryManager = memoryManager;
            this.requestSendPeriod = requestSendPeriod;
            this.requestSendPeriodCap = requestSendPeriodCap;

            attempts = new Dictionary<string, int>();
        }

        public ISchedule GetDelayToNextOccurrence(string stream, bool lastSendingResult, TimeSpan lastSendingElapsed)
        {
            if (lastSendingResult && memoryManager.IsConsumptionAchievedThreshold(50))
                return new Schedule(TimeSpan.Zero);

            attempts[stream] = CalculateAttempt(stream, lastSendingResult);
            var sendPeriod = Delays.Exponential(requestSendPeriodCap, requestSendPeriod, attempts[stream]).WithEqualJitter().Value;
            var delayToNextOccurrence = lastSendingResult ? sendPeriod - lastSendingElapsed : sendPeriod;

            return new Schedule(delayToNextOccurrence);
        }

        private int CalculateAttempt(string stream, bool lastSendingResult) =>
            lastSendingResult
                ? 0
                : attempts.TryGetValue(stream, out var attempt)
                    ? attempt + 1
                    : 1;
    }
}