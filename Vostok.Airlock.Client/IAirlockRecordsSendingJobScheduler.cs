namespace Vostok.Airlock.Client
{
    internal interface IAirlockRecordsSendingJobScheduler
    {
        ISchedule GetDelayToNextOccurrence(AirlockRecordsSendingJobState jobState);
    }
}