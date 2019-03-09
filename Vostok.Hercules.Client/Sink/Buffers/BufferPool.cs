using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vostok.Commons.Threading;

namespace Vostok.Hercules.Client.Sink.Buffers
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
            int initialBufferSize,
            int maxRecordSize,
            int maxBufferSize)
        {
            this.memoryManager = memoryManager;
            this.initialBufferSize = initialBufferSize;
            this.maxRecordSize = maxRecordSize;
            this.maxBufferSize = maxBufferSize;
        }

        public AsyncManualResetEvent NeedToFlushEvent { get; } = new AsyncManualResetEvent(false);

        public bool TryAcquire(out IBuffer buffer)
        {
            var result = TryDequeueBuffer(out buffer) || TryCreateBuffer(out buffer);

            if (result)
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

        public (long count, long size) GetStoredRecordsStatistics()
        {
            long count = 0, size = 0;

            foreach (var buffer in this)
            {
                var state = buffer.GetState();
                count += state.RecordsCount;
                size += state.Length;
            }

            return (count, size);
        }

        public IEnumerator<IBuffer> GetEnumerator() => allBuffers.GetEnumerator();

        private bool TryDequeueBuffer(out IBuffer buffer)
        {
            const int maxDequeueAttempts = 10;

            var dequeueAttempts = Math.Min(maxDequeueAttempts, buffers.Count);

            for (var i = 0; i < dequeueAttempts; ++i)
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

        private bool TryCreateBuffer(out IBuffer buffer)
        {
            if (!memoryManager.TryReserveBytes(initialBufferSize))
            {
                buffer = null;
                return false;
            }

            buffer = new Buffer(initialBufferSize, memoryManager);
            buffer.TryLock();

            allBuffers.Enqueue(buffer);

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() =>
            GetEnumerator();
    }
}