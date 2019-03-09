using JetBrains.Annotations;

namespace Vostok.Hercules.Client
{
    /// <summary>
    /// <para>Provides diagnostics information about <see cref="HerculesSink"/> overall or about one of the streams in it.</para>
    /// </summary>
    [PublicAPI]
    public class HerculesSinkStatistics
    {
        /// <summary>
        /// Statistics about records that already have been sent.
        /// </summary>
        public (long Count, long Size) SentRecords { get; set; }

        /// <summary>
        /// Statistics about records that lost due to non-retriable sending errors.
        /// </summary>
        public (long Count, long Size) LostRecords { get; set; }

        /// <summary>
        /// Statistics about records that stored inside internal buffers and waiting to be sent.
        /// </summary>
        public (long Count, long Size) StoredRecords { get; set; }

        /// <summary>
        /// How many records are lost due to exception in record building delegate.
        /// </summary>
        public long WriteFailuresCount { get; set; }

        /// <summary>
        /// How many records are lost because they are large than <see cref="HerculesSinkSettings.MaximumRecordSize"/>.
        /// </summary>
        public long TooLargeRecordsCount { get; set; }

        /// <summary>
        /// How many records are lost due to memory limit violation.
        /// </summary>
        public long OverflowsCount { get; set; }
    }
}