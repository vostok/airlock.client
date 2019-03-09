namespace Vostok.Hercules.Client.Sink
{
    internal interface IStatisticsCollector
    {
        HerculesSinkStatistics Get();
        void ReportTooLargeRecord();
        void ReportWriteFailure();
        void ReportOverflow();
        void ReportSendingFailure(long count, long size);
        void ReportSuccessfulSending(long count, long size);
        void ReportWrittenRecord(long size);
        long EstimateStoredSize();
    }
}