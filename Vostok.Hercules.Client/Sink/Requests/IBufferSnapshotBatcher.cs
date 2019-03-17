using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal interface IBufferSnapshotBatcher
    {
        /// <summary>
        /// Splits given <paramref name="snapshots"/> into batches, each of which fits into configured max batch size.
        /// </summary>
        [NotNull]
        [ItemNotNull]
        IEnumerable<IReadOnlyList<BufferSnapshot>> Batch([NotNull] IEnumerable<BufferSnapshot> snapshots);
    }
}