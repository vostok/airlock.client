using System;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client
{
    internal interface IHerculesRecordsSendingJobScheduler
    {
        TimeSpan GetDelayToNextOccurrence(string stream, bool lastSendingResult, TimeSpan lastSendingElapsed);
    }
}