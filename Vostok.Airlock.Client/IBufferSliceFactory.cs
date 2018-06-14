using System.Collections.Generic;

namespace Vostok.Airlock.Client
{
    internal interface IBufferSliceFactory
    {
        IEnumerable<BufferSlice> Cut(BufferSnapshot snapshot);
    }
}