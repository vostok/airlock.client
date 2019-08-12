using System.Collections.Generic;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IBufferPool : IEnumerable<IBuffer>
    {
        bool TryAcquire(out IBuffer buffer);
        void Release(IBuffer buffer);
        void Free(IBuffer buffer);
        long EstimateReservedMemorySize();
        long LastReserveMemoryTicks();
    }
}