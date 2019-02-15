using System.Collections.Generic;
using System.Threading.Tasks;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.Hercules.Client
{
    internal interface IBufferPool
    {
        StreamSettings Settings { get; set; }
        bool TryAcquire(out IBuffer buffer);
        void Release(IBuffer buffer);
        IReadOnlyCollection<IBuffer> MakeSnapshot();
        long GetStoredRecordsCount();
        AsyncManualResetEvent NeedToFlushEvent { get; }
    }
}