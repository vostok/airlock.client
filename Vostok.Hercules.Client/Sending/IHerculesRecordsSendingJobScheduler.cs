using System;

namespace Vostok.Hercules.Client.Sending
{
    internal interface IHerculesRecordsSendingJobScheduler
    {
        TimeSpan GetDelayToNextOccurrence(string stream, bool lastSendingResult, TimeSpan lastSendingElapsed);
    }
}