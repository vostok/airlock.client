using System;

namespace Vostok.Airlock.Client
{
    internal interface IAirlockRecordsSendingJobScheduler
    {
        ISchedule GetDelayToNextOccurrence(string stream, bool lastSendingResult, TimeSpan lastSendingElapsed);
    }
}