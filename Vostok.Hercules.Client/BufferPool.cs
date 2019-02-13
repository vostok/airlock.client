using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Vostok.Commons.Threading;

namespace Vostok.Hercules.Client
{
    internal class BufferPool : IBufferPool
    {
        private readonly IMemoryManager memoryManager;
        private readonly int initialBufferSize;
        private readonly int maxRecordSize;
        private readonly int maxBufferSize;

        private readonly ConcurrentQueue<IBuffer> buffers = new ConcurrentQueue<IBuffer>();
        private readonly ConcurrentQueue<IBuffer> allBuffers = new ConcurrentQueue<IBuffer>();

        public BufferPool(
            IMemoryManager memoryManager,
            int initialCount,
            int initialBufferSize,
            int maxRecordSize,
            int maxBufferSize)
        {
            this.memoryManager = memoryManager;
            this.initialBufferSize = initialBufferSize;
            this.maxRecordSize = maxRecordSize;
            this.maxBufferSize = maxBufferSize;

            for (var i = 0; i < initialCount; i++)
            {
                if (TryCreateBuffer(out var buffer, false))
                    buffers.Enqueue(buffer);
                else
                    break;
            }
        }

        public bool TryAcquire(out IBuffer buffer)
        {
            var result = TryDequeueBuffer(out buffer) || TryCreateBuffer(out buffer, true);

            if (result) // we can collect garbage not on every iteration
                buffer.CollectGarbage();

            return result;
        }

        public void Release(IBuffer buffer)
        {
            var needToFlush = buffer.GetState().Length > maxBufferSize / 4;
            
            buffer.Unlock();
            buffers.Enqueue(buffer);
            
            if (needToFlush)
                NeedToFlushEvent.Set();
        }

        public long GetStoredRecordsCount() => buffers.Sum(x => x.GetState().RecordsCount);
        
        public AsyncManualResetEvent NeedToFlushEvent { get; } = new AsyncManualResetEvent(false);
        
        public IReadOnlyCollection<IBuffer> MakeSnapshot()
        {
            var snapshot = null as List<IBuffer>;

            foreach (var buffer in allBuffers)
            {
                if (buffer.HasGarbage())
                {
                    if (!buffer.TryLock())
                        continue;

                    buffer.CollectGarbage();
                    buffer.Unlock();
                }

                if (!buffer.IsEmpty())
                    (snapshot ?? (snapshot = new List<IBuffer>())).Add(buffer);
            }

            return snapshot;
        }

        private bool TryDequeueBuffer(out IBuffer buffer)
        {
            var dequeueAttempts = Math.Min(3, buffers.Count);

            for (var i = 0; i < dequeueAttempts; i++)
            {
                if (!buffers.TryDequeue(out buffer))
                    return false;

                var state = buffer.GetState();

                if (state.Length <= maxBufferSize - maxRecordSize && buffer.TryLock())
                    return true;

                buffers.Enqueue(buffer);
            }

            buffer = null;
            return false;
        }

        private bool TryCreateBuffer(out IBuffer buffer, bool lockCreatedBuffer)
        {
            if (!memoryManager.TryReserveBytes(initialBufferSize))
            {
                buffer = null;
                return false;
            }

            buffer = new Buffer(initialBufferSize, memoryManager);

            allBuffers.Enqueue(buffer);

            if (lockCreatedBuffer)
                buffer.TryLock();

            return true;
        }
    }
}