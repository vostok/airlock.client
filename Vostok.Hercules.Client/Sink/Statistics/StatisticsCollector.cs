using System.Threading;

namespace Vostok.Hercules.Client.Sink.Statistics
{
    internal class StatisticsCollector : IStatisticsCollector
    {
        private long writeFailures;
        private long overflows;
        private long tooLargeRecords;
        private long lostRecordsCount;
        private long lostRecordsSize;
        private long sentRecordsCount;
        private long sentRecordsSize;
        private long storedRecordsCount;
        private long storedRecordsSize;

        public HerculesSinkCounters Get()
        {
            return new HerculesSinkCounters
            {
                LostRecords = ReadTuple(ref lostRecordsCount, ref lostRecordsSize),
                SentRecords = ReadTuple(ref sentRecordsCount, ref sentRecordsSize),
                StoredRecords = ReadTuple(ref storedRecordsCount, ref storedRecordsSize),
                WriteFailuresCount = Interlocked.Read(ref writeFailures)
            };
        }

        public void ReportTooLargeRecord() => Interlocked.Increment(ref tooLargeRecords);

        public void ReportWriteFailure() => Interlocked.Increment(ref writeFailures);

        public void ReportOverflow() => Interlocked.Increment(ref overflows);

        public void ReportSendingFailure(long count, long size)
        {
            Interlocked.Add(ref lostRecordsCount, count);
            Interlocked.Add(ref lostRecordsSize, size);

            Interlocked.Add(ref storedRecordsCount, count);
            Interlocked.Add(ref sentRecordsSize, size);
        }

        public void ReportSuccessfulSending(long count, long size)
        {
            Interlocked.Add(ref sentRecordsCount, count);
            Interlocked.Add(ref sentRecordsSize, size);

            Interlocked.Add(ref storedRecordsCount, count);
            Interlocked.Add(ref sentRecordsSize, size);
        }

        public void ReportWrittenRecord(long size)
        {
            Interlocked.Increment(ref storedRecordsCount);
            Interlocked.Add(ref storedRecordsSize, size);
        }

        public long EstimateStoredSize() => Interlocked.Read(ref storedRecordsSize);

        private static (long, long) ReadTuple(ref long a, ref long b) =>
            (Interlocked.Read(ref a), Interlocked.Read(ref b));
    }
}