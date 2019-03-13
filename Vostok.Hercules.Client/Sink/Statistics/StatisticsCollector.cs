using System.Threading;

namespace Vostok.Hercules.Client.Sink.Statistics
{
    internal class StatisticsCollector : IStatisticsCollector
    {
        private long buildFailures;
        private long overflows;
        private long sizeLimitViolations;
        private long lostRecordsCount;
        private long lostRecordsSize;
        private long sentRecordsCount;
        private long sentRecordsSize;
        private long storedRecordsCount;
        private long storedRecordsSize;

        public HerculesSinkCounters Get()
        {
            return new HerculesSinkCounters(
                ReadTuple(ref sentRecordsCount, ref sentRecordsSize),
                ReadTuple(ref lostRecordsCount, ref lostRecordsSize),
                ReadTuple(ref storedRecordsCount, ref storedRecordsSize),
                Interlocked.Read(ref buildFailures),
                Interlocked.Read(ref sizeLimitViolations),
                Interlocked.Read(ref overflows));
        }

        public void ReportSizeLimitViolation() => Interlocked.Increment(ref sizeLimitViolations);

        public void ReportRecordBuildFailure() => Interlocked.Increment(ref buildFailures);

        public void ReportOverflow() => Interlocked.Increment(ref overflows);

        public void ReportSendingFailure(long count, long size)
        {
            Interlocked.Add(ref lostRecordsCount, count);
            Interlocked.Add(ref lostRecordsSize, size);

            Interlocked.Add(ref storedRecordsCount, -count);
            Interlocked.Add(ref storedRecordsSize, -size);
        }

        public void ReportSuccessfulSending(long count, long size)
        {
            Interlocked.Add(ref sentRecordsCount, count);
            Interlocked.Add(ref sentRecordsSize, size);

            Interlocked.Add(ref storedRecordsCount, -count);
            Interlocked.Add(ref storedRecordsSize, -size);
        }

        public void ReportStoredRecord(long size)
        {
            Interlocked.Increment(ref storedRecordsCount);
            Interlocked.Add(ref storedRecordsSize, size);
        }

        public long EstimateStoredSize() => Interlocked.Read(ref storedRecordsSize);

        private static (long, long) ReadTuple(ref long a, ref long b) =>
            (Interlocked.Read(ref a), Interlocked.Read(ref b));
    }
}