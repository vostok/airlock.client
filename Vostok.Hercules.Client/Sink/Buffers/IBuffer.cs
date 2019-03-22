using JetBrains.Annotations;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    /// <summary>
    /// <para><see cref="IBuffer"/> thread safety is based on usage assumptions listed below.</para>
    /// <para>There is a single sender thread that periodically performs a following sequence of calls: <see cref="TryMakeSnapshot"/> --> <see cref="ReportGarbage"/>.</para>
    /// <para>There can also be at most one writer thread at any given moment, operating concurrently with sender thread.</para>
    /// <para>Writer thread may collect garbage, write data and issue <see cref="CommitRecord"/> calls.</para>
    /// </summary>
    internal interface IBuffer : IBinaryWriter
    {
        /// <summary>
        /// Gets or sets whether this buffer has reached its maximum size and can no longer be written to.
        /// </summary>
        bool IsOverflowed { get; set; }

        /// <summary>
        /// Returns total current size of committed records excepts garbage reported with <see cref="ReportGarbage"/>.
        /// </summary>
        long UsefulDataSize { get; }

        /// <summary>
        /// Commits a recently written record of given <paramref name="size"/>, so that it will be included in result of the next successful <see cref="TryMakeSnapshot"/> call.
        /// </summary>
        /// <exception cref="System.ArgumentOutOfRangeException">Provided <paramref name="size"/> is zero or negative.</exception>
        /// <exception cref="System.InvalidOperationException">Committed region would exceed current physical buffer size.</exception>
        void CommitRecord(int size);

        /// <summary>
        /// <para>Marks given <paramref name="region"/> of committed records as garbage.</para>
        /// <para>Consequent <see cref="TryMakeSnapshot"/> calls may collect this garbage.</para>
        /// </summary>
        /// <exception cref="System.InvalidOperationException">Buffer already has garbage.</exception>
        /// <exception cref="System.InvalidOperationException">Given garbage <paramref name="region"/>'s length exceeds current committed length.</exception>
        /// <exception cref="System.InvalidOperationException">Given garbage <paramref name="region"/>'s records count exceeds current committed records count.</exception>
        void ReportGarbage(BufferState region);

        /// <summary>
        /// <para>Attempts to collect garbage (if any) and return a snapshot pointing to the committed region of this buffer.</para>
        /// <para>May return <c>null</c> if buffer contains garbage that could not be collected right away.</para>
        /// <para>May return an empty snapshot if buffer currently contains no committed records.</para>
        /// <para>If this method returns a non-null <see cref="BufferSnapshot"/>, it's guaranteed that data pointed by this snapshot does not contain garbage records.</para>
        /// </summary>
        [CanBeNull]
        BufferSnapshot TryMakeSnapshot();
    }
}