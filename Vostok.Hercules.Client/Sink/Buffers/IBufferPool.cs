using System.Collections.Generic;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IBufferPool : IEnumerable<IBuffer>
    {
        bool TryAcquire(out IBuffer buffer);
        void Release(IBuffer buffer);
        void Free(IBuffer buffer);

        // CR(iloktionov): I feel like we shouldn't proxy these calls through the buffer pool. Why not give our MemoryAnalyzer direct access to MemoryManager?
        long EstimateReservedMemorySize();
        long LastReserveMemoryTicks();
    }
}