using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.Commons.Threading;

namespace Vostok.Hercules.Client
{
    internal interface IBufferPool
    {
        bool TryAcquire(out IBuffer buffer);
        void Release(IBuffer buffer);
        IReadOnlyCollection<IBuffer> MakeSnapshot();
        long GetStoredRecordsCount();
        AsyncManualResetEvent NeedToFlushEvent { get; }
    }
}