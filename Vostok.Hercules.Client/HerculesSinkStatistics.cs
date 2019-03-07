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
        /// How many records have already been sent.
        /// </summary>
        public long SentRecordsCount { get; set; }
        
        /// <summary>
        /// How many records are lost due to memory limit violation and network communication errors.
        /// </summary>
        public long LostRecordsCount { get; set; }
        
        /// <summary>
        /// How many records stored inside internal buffers and waiting to be sent.
        /// </summary>
        public long StoredRecordsCount { get; set; }

        /// <summary>
        /// How many bytes of records have already been sent.
        /// </summary>
        public long SentRecordsSize { get; set; }

        /// <summary>
        /// How many bytes of records are lost due to memory limit violation and network communication errors.
        /// </summary>
        public long LostRecordsSize { get; set; }

        /// <summary>
        /// How many bytes of records are stored inside internal buffers and waiting to be sent.
        /// </summary>
        public long StoredRecordsSize { get; set; }
    }
}