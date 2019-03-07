using System.Collections.Generic;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions.Models;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal interface IBufferPool : IEnumerable<IBuffer>
    {
        StreamSettings Settings { get; set; }
        AsyncManualResetEvent NeedToFlushEvent { get; }
        bool TryAcquire(out IBuffer buffer);
        void Release(IBuffer buffer);
        long GetStoredRecordsCount();
    }
}