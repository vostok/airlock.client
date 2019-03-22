using System.Threading;

namespace Vostok.Hercules.Client.Sink.Statistics
{
    internal class StatisticsCollector : IStatisticsCollector
    {
        private long buildFailures;
        private long overflows;
        private long sizeLimitViolations;
        private long rejectedRecordsCount;
        private long rejectedRecordsSize;
        private long sentRecordsCount;
        private long sentRecordsSize;
        private long storedRecordsCount;
        private long storedRecordsSize;

        public HerculesSinkCounters GetCounters()
            => new HerculesSinkCounters(
                ReadTuple(ref sentRecordsCount, ref sentRecordsSize),
                ReadTuple(ref rejectedRecordsCount, ref rejectedRecordsSize),
                ReadTuple(ref storedRecordsCount, ref storedRecordsSize),
                Interlocked.Read(ref buildFailures),
                Interlocked.Read(ref sizeLimitViolations),
                Interlocked.Read(ref overflows));

        public long EstimateStoredSize()
            => Interlocked.Read(ref storedRecordsSize);

        public void ReportSizeLimitViolation()
            => Interlocked.Increment(ref sizeLimitViolations);

        public void ReportRecordBuildFailure()
            => Interlocked.Increment(ref buildFailures);

        public void ReportOverflow()
            => Interlocked.Increment(ref overflows);

        public void ReportSuccessfulSending(long count, long size)
        {
            Interlocked.Add(ref sentRecordsCount, count);
            Interlocked.Add(ref sentRecordsSize, size);

            Interlocked.Add(ref storedRecordsCount, -count);
            Interlocked.Add(ref storedRecordsSize, -size);
        }

        public void ReportSendingFailure(long count, long size)
        {
            Interlocked.Add(ref rejectedRecordsCount, count);
            Interlocked.Add(ref rejectedRecordsSize, size);

            Interlocked.Add(ref storedRecordsCount, -count);
            Interlocked.Add(ref storedRecordsSize, -size);
        }

        public void ReportStoredRecord(long size)
        {
            Interlocked.Increment(ref storedRecordsCount);
            Interlocked.Add(ref storedRecordsSize, size);
        }

        private static (long, long) ReadTuple(ref long first, ref long second) =>
            (Interlocked.Read(ref first), Interlocked.Read(ref second));
    }
}