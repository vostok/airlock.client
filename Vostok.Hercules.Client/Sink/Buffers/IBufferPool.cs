using System.Collections.Generic;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Models;

namespace Vostok.Hercules.Client.Sink.Buffers
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