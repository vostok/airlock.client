using System;
using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    internal interface IBufferSnapshotBatcher
    {
        IEnumerable<ArraySegment<BufferSnapshot>> Batch(BufferSnapshot[] snapshots);
    }
}