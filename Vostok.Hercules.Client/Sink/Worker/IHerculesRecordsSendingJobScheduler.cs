using System;

namespace Vostok.Hercules.Client.Sink.Worker
{
    internal interface IHerculesRecordsSendingJobScheduler
    {
        TimeSpan GetDelayToNextOccurrence(string stream, bool lastSendingResult, TimeSpan lastSendingElapsed);
    }
}