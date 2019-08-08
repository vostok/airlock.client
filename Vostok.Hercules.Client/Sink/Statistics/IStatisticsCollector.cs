using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Sink.Statistics
{
    internal interface IStatisticsCollector
    {
        /// <summary>
        /// Returns current accumulated counters.
        /// </summary>
        [NotNull]
        HerculesSinkCounters GetCounters();

        /// <summary>
        /// Estimates the size of currently stored records without garbage.
        /// </summary>
        long EstimateStoredSize();

        /// <summary>
        /// Reports that a record of given <paramref name="size"/> has been stored into a buffer.
        /// </summary>
        void ReportStoredRecord(long size);

        /// <summary>
        /// Reports that <paramref name="count"/> records of total <paramref name="size"/> have been successfuly sent.
        /// </summary>
        void ReportSuccessfulSending(long count, long size);

        /// <summary>
        /// Reports that <paramref name="count"/> records of total <paramref name="size"/> have been lost due to a non-retriable failure.
        /// </summary>
        void ReportSendingFailure(long count, long size);

        /// <summary>
        /// Reports that a record could not be stored due to buffer overflow.
        /// </summary>
        void ReportOverflow();

        /// <summary>
        /// Reports that a record could not be stored due to being too large.
        /// </summary>
        void ReportSizeLimitViolation();

        /// <summary>
        /// Reports that a record could not be stored due to an exception.
        /// </summary>
        void ReportRecordBuildFailure();

        /// <summary>
        /// Reports how many bytes reserved by buffer pool currently. 
        /// </summary>
        void ReportReservedSize(long amount);
    }
}