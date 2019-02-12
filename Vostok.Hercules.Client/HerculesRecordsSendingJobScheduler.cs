using System;
using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordsSendingJobScheduler : IHerculesRecordsSendingJobScheduler
    {
        private readonly IMemoryManager memoryManager;
        private readonly TimeSpan requestSendPeriod;
        private readonly TimeSpan requestSendPeriodCap;

        private readonly Dictionary<string, int> attempts;

        public HerculesRecordsSendingJobScheduler(IMemoryManager memoryManager, TimeSpan requestSendPeriod, TimeSpan requestSendPeriodCap)
        {
            this.memoryManager = memoryManager;
            this.requestSendPeriod = requestSendPeriod;
            this.requestSendPeriodCap = requestSendPeriodCap;

            attempts = new Dictionary<string, int>();
        }

        public TimeSpan GetDelayToNextOccurrence(string stream, bool lastSendingResult, TimeSpan lastSendingElapsed)
        {
            if (lastSendingResult && memoryManager.IsConsumptionAchievedThreshold(50))
                return TimeSpan.Zero;

            attempts[stream] = CalculateAttempt(stream, lastSendingResult);
            var sendPeriod = Delays.ExponentialWithJitter(requestSendPeriodCap, requestSendPeriod, attempts[stream]);
            var delayToNextOccurrence = lastSendingResult ? sendPeriod - lastSendingElapsed : sendPeriod;

            return delayToNextOccurrence;
        }

        private int CalculateAttempt(string stream, bool lastSendingResult) =>
            lastSendingResult
                ? 0
                : attempts.TryGetValue(stream, out var attempt)
                    ? attempt + 1
                    : 1;
    }
}