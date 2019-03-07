using System;
using System.Collections.Generic;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Worker
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
            attempts[stream] = CalculateAttempt(stream, lastSendingResult);
            var sendPeriod = Delays.ExponentialWithJitter(requestSendPeriodCap, requestSendPeriod, attempts[stream]);
            var delayToNextOccurrence = lastSendingResult ? sendPeriod - lastSendingElapsed : sendPeriod;

            return delayToNextOccurrence;
        }

        private int CalculateAttempt(string stream, bool isLastSendingSuccessful)
        {
            if (isLastSendingSuccessful)
                return 0;
            return attempts.TryGetValue(stream, out var attempt)
                ? Math.Min(int.MaxValue - 1, attempt + 1)
                : 1;
        }
    }
}