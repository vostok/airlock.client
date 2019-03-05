using System;
using System.Collections.Generic;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal interface IBufferSnapshotBatcher
    {
        IEnumerable<ArraySegment<BufferSnapshot>> Batch(BufferSnapshot[] snapshots);
    }
}