using System;

namespace Vostok.Hercules.Client
{
    internal interface IHerculesRecordsSendingJobScheduler
    {
        ISchedule GetDelayToNextOccurrence(string stream, bool lastSendingResult, TimeSpan lastSendingElapsed);
    }
}