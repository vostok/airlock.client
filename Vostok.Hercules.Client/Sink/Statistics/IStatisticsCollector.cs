namespace Vostok.Hercules.Client.Sink.Statistics
{
    internal interface IStatisticsCollector
    {
        HerculesSinkCounters Get();
        void ReportSizeLimitViolation();
        void ReportRecordBuildFailure();
        void ReportOverflow();
        void ReportSendingFailure(long count, long size);
        void ReportSuccessfulSending(long count, long size);
        void ReportStoredRecord(long size);
        long EstimateStoredSize();
    }
}