using JetBrains.Annotations;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// <see cref="HerculesSinkCounters"/> contain a set of cumulative counters describing internal processes of a <see cref="HerculesSink"/>.
    /// </summary>
    [PublicAPI]
    public class HerculesSinkCounters
    {
        public static readonly HerculesSinkCounters Zero
            = new HerculesSinkCounters((0, 0), (0, 0), (0, 0), 0, 0, 0);

        public HerculesSinkCounters(
            (long Count, long Size) sentRecords,
            (long Count, long Size) rejectedRecords,
            (long Count, long Size) storedRecords,
            long recordsLostDueToBuildFailures,
            long recordsLostDueToSizeLimit,
            long recordsLostDueToOverflows)
        {
            SentRecords = sentRecords;
            RejectedRecords = rejectedRecords;
            StoredRecords = storedRecords;
            RecordsLostDueToBuildFailures = recordsLostDueToBuildFailures;
            RecordsLostDueToSizeLimit = recordsLostDueToSizeLimit;
            RecordsLostDueToOverflows = recordsLostDueToOverflows;
        }

        /// <summary>
        /// Records that have been successfully sent.
        /// </summary>
        public (long Count, long Size) SentRecords { get; }

        /// <summary>
        /// Records that have been lost due to non-retriable sending errors.
        /// </summary>
        public (long Count, long Size) RejectedRecords { get; }

        /// <summary>
        /// Records that are currently stored in internal buffers and waiting to be sent.
        /// </summary>
        public (long Count, long Size) StoredRecords { get; }

        /// <summary>
        /// Returns how many records have been lost in total, whatever the reason.
        /// </summary>
        public long TotalLostRecords =>
            RejectedRecords.Count +
            RecordsLostDueToOverflows +
            RecordsLostDueToSizeLimit +
            RecordsLostDueToBuildFailures;

        /// <summary>
        /// Returns how many records have been lost due to exceptions in user-provided record building delegates.
        /// </summary>
        public long RecordsLostDueToBuildFailures { get; }

        /// <summary>
        /// Returns how many records have been lost due to being larger than <see cref="HerculesSinkSettings.MaximumRecordSize"/>.
        /// </summary>
        public long RecordsLostDueToSizeLimit { get; }

        /// <summary>
        /// Returns how many records have been lost due to memory limit violations.
        /// </summary>
        public long RecordsLostDueToOverflows { get; }

        /// <summary>
        /// Sums two <see cref="HerculesSinkCounters"/> field-by-field.
        /// </summary>
        public static HerculesSinkCounters operator+(HerculesSinkCounters left, HerculesSinkCounters right)
        {
            return new HerculesSinkCounters(
                Sum(left.SentRecords, right.SentRecords),
                Sum(left.RejectedRecords, right.RejectedRecords),
                Sum(left.StoredRecords, right.StoredRecords),
                left.RecordsLostDueToBuildFailures + right.RecordsLostDueToBuildFailures,
                left.RecordsLostDueToSizeLimit + right.RecordsLostDueToSizeLimit,
                left.RecordsLostDueToOverflows + right.RecordsLostDueToOverflows);

            (long, long) Sum((long, long) x, (long, long) y) => (x.Item1 + y.Item1, x.Item2 + y.Item2);
        }
    }
}