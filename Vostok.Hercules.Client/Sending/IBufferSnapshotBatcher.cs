using System;
using System.Collections.Generic;
using Vostok.Hercules.Client.Sink;

namespace Vostok.Hercules.Client.Sending
{
    internal interface IBufferSnapshotBatcher
    {
        IEnumerable<ArraySegment<BufferSnapshot>> Batch(BufferSnapshot[] snapshots);
    }
}