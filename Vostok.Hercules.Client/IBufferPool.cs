using System.Collections.Generic;

namespace Vostok.Hercules.Client
{
    internal interface IBufferPool
    {
        bool TryAcquire(out IBuffer buffer);
        void Release(IBuffer buffer);
        IReadOnlyCollection<IBuffer> MakeSnapshot();
    }
}