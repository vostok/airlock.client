using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    internal interface IBufferSliceFactory
    {
        IEnumerable<BufferSlice> Cut(BufferSnapshot snapshot);
    }
}