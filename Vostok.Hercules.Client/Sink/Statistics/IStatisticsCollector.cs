namespace Vostok.Hercules.Client.Sink.Statistics
{
    internal interface IStatisticsCollector
    {
        HerculesSinkCounters Get();
        void ReportTooLargeRecord();
        void ReportWriteFailure();
        void ReportOverflow();
        void ReportSendingFailure(long count, long size);
        void ReportSuccessfulSending(long count, long size);
        void ReportWrittenRecord(long size);
        long EstimateStoredSize();
    }
}