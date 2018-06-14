using System.Collections.Generic;

namespace Vostok.Airlock.Client
{
    internal interface IBufferPool
    {
        bool TryAcquire(out IBuffer buffer);
        void Release(IBuffer buffer);
        List<IBuffer> MakeSnapshot();
    }
}