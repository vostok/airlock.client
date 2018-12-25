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
        private readonly HashSet<IBuffer> sieve;

        public BufferPool(IMemoryManager memoryManager, int initialCount, int initialBufferSize)
        {
            this.memoryManager = memoryManager;
            this.initialBufferSize = initialBufferSize;

            buffers = new ConcurrentQueue<IBuffer>();
            sieve = new HashSet<IBuffer>();

            for (var i = 0; i < initialCount; i++)
            {
                if (TryCreateBuffer(out var buffer))
                    buffers.Enqueue(buffer);
                else
                    break;
            }
        }

        public bool TryAcquire(out IBuffer buffer)
        {
            var result = buffers.TryDequeue(out buffer) || TryCreateBuffer(out buffer);

            if (result) // we can collect garbage not on every iteration
                buffer.CollectGarbage();

            return result;
        }

        public void Release(IBuffer buffer) => buffers.Enqueue(buffer);

        public long GetStoredRecordsCount() => buffers.Sum(x => x.EstimateRecordsCountForMonitoring());
        
        /// <summary>
        /// <threadsafety>This method is NOT threadsafe and should be called only from <see cref="HerculesRecordsSendingJob"/>.</threadsafety>
        /// </summary>
        public IReadOnlyCollection<IBuffer> MakeSnapshot()
        {
            sieve.Clear();

            var initialCount = buffers.Count;
            var snapshot = null as List<IBuffer>;

            for (var i = 0; i < initialCount * 2; i++)
            {
                if (!buffers.TryDequeue(out var buffer))
                    break;

                if (!sieve.Add(buffer))
                {
                    buffers.Enqueue(buffer);
                    continue;
                }

                buffer.CollectGarbage();

                if (!buffer.IsEmpty())
                    (snapshot ?? (snapshot = new List<IBuffer>())).Add(buffer);

                buffers.Enqueue(buffer);
            }

            return snapshot;
        }

        private bool TryCreateBuffer(out IBuffer buffer)
        {
            if (!memoryManager.TryReserveBytes(initialBufferSize))
            {
                buffer = null;
                return false;
            }

            buffer = new Buffer(initialBufferSize, memoryManager);
            return true;
        }
    }
}