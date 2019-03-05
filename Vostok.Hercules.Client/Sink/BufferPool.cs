using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions;

namespace Vostok.Hercules.Client.Sink
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

        public StreamSettings Settings { get; set; } = new StreamSettings();

        public AsyncManualResetEvent NeedToFlushEvent { get; } = new AsyncManualResetEvent(false);

        public bool TryAcquire(out IBuffer buffer)
        {
            var result = TryDequeueBuffer(out buffer) || TryCreateBuffer(out buffer);

            if (result) // we can collect garbage not on every iteration
                buffer.CollectGarbage();

            return result;
        }

        public void Release(IBuffer buffer)
        {
            var needToFlush = buffer.GetState().Length > maxBufferSize / 4;

            buffers.Enqueue(buffer);

            if (needToFlush)
                NeedToFlushEvent.Set();
        }

        public long GetStoredRecordsCount() => buffers.Sum(x => x.GetState().RecordsCount);

        [CanBeNull]
        public IReadOnlyCollection<IBuffer> MakeSnapshot()
        {
            var snapshot = null as List<IBuffer>;

            if (allBuffers.Any(x => x.HasGarbage()))
                CollectGarbageFromAllBuffers();

            foreach (var buffer in allBuffers)
            {
                if (buffer.HasGarbage())
                    continue;

                if (!buffer.IsEmpty())
                    (snapshot ?? (snapshot = new List<IBuffer>())).Add(buffer);
            }

            return snapshot;
        }

        private bool TryDequeueBuffer(out IBuffer buffer)
        {
            if (!buffers.TryDequeue(out buffer))
                return false;

            var state = buffer.GetState();

            if (state.Length <= maxBufferSize - maxRecordSize)
                return true;

            buffers.Enqueue(buffer);
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

            allBuffers.Enqueue(buffer);

            return true;
        }

        private void CollectGarbageFromAllBuffers()
        {
            var count = allBuffers.Count;
            for (var i = 0; i < count; i++)
            {
                // garbage collection from buffers is not thread safe,
                // so buffer should not be available for write.
                if (!buffers.TryDequeue(out var buffer))
                    continue;

                buffer.CollectGarbage();
                buffers.Enqueue(buffer);
            }
        }
    }
}