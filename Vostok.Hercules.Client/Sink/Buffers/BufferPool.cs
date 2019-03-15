using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal class BufferPool : IBufferPool
    {
        private readonly IMemoryManager memoryManager;
        private readonly int initialBufferSize;
        private readonly int maxRecordSize;
        private readonly int maxBufferSize;

        private readonly ConcurrentQueue<IBuffer> availableBuffers = new ConcurrentQueue<IBuffer>();
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

        public bool TryAcquire(out IBuffer buffer)
        {
            var result = TryDequeueBuffer(out buffer) || TryCreateBuffer(out buffer);
            if (result)
                buffer.CollectGarbage();

            return result;
        }

        public void Release(IBuffer buffer)
        {
            Unlock(buffer);

            availableBuffers.Enqueue(buffer);
        }

        public IEnumerator<IBuffer> GetEnumerator() => allBuffers.GetEnumerator();

        private static void Unlock(IBuffer buffer) => (buffer as Buffer)?.Unlock();
        private static bool TryLock(IBuffer buffer) => (buffer as Buffer)?.TryLock() ?? true;

        private bool TryDequeueBuffer(out IBuffer buffer)
        {
            const int dequeueAttempts = 3;

            for (var i = 0; i < dequeueAttempts; ++i)
            {
                if (!availableBuffers.TryDequeue(out buffer))
                    return false;

                if (buffer.UsefulDataSize <= maxBufferSize - maxRecordSize && TryLock(buffer))
                    return true;

                availableBuffers.Enqueue(buffer);
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

            buffer = new Buffer(initialBufferSize, maxBufferSize, memoryManager);
            TryLock(buffer);

            allBuffers.Enqueue(buffer);

            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}