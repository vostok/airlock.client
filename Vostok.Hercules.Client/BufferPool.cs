using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Hercules.Client
{
    internal class BufferPool : IBufferPool
    {
        private readonly IMemoryManager memoryManager;
        private readonly int initialBufferSize;

        private readonly ConcurrentQueue<IBuffer> buffers;
        private readonly ConcurrentQueue<IBuffer> allBuffers;

        public BufferPool(IMemoryManager memoryManager, int initialCount, int initialBufferSize)
        {
            this.memoryManager = memoryManager;
            this.initialBufferSize = initialBufferSize;

            allBuffers = new ConcurrentQueue<IBuffer>();
            buffers = new ConcurrentQueue<IBuffer>();

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
            buffer.Unlock();
            buffers.Enqueue(buffer);
        }

        public long GetStoredRecordsCount() => buffers.Sum(x => x.EstimateRecordsCountForMonitoring());

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
            var count = buffers.Count;

            for (var i = 0; i < count; i++)
            {
                if (!buffers.TryDequeue(out buffer))
                    return false;

                if (buffer.TryLock())
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