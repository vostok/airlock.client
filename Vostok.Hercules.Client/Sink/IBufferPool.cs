using System.Collections.Generic;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.Hercules.Client.Sink
{
    internal interface IBufferPool
    {
        StreamSettings Settings { get; set; }
        AsyncManualResetEvent NeedToFlushEvent { get; }
        bool TryAcquire(out IBuffer buffer);
        void Release(IBuffer buffer);
        IReadOnlyCollection<IBuffer> MakeSnapshot();
        long GetStoredRecordsCount();
    }
}