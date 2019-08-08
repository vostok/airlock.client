﻿using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Vostok.Commons.Collections;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal class BufferPool : IBufferPool
    {
        private readonly IMemoryManager memoryManager;
        private readonly int initialBufferSize;
        private readonly int maxRecordSize;
        private readonly int maxBufferSize;

        private readonly ConcurrentQueue<IBuffer> availableBuffers = new ConcurrentQueue<IBuffer>();
        private readonly ConcurrentDictionary<IBuffer, byte> allBuffers = new ConcurrentDictionary<IBuffer, byte>(ByReferenceEqualityComparer<IBuffer>.Instance);

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
                (buffer as Buffer)?.CollectGarbage();

            return result;
        }

        public void Release(IBuffer buffer)
        {
            Unlock(buffer);

            availableBuffers.Enqueue(buffer);
        }

        public long EstimateReservedSize() =>
            memoryManager.EstimateReservedBytes();

        public void Free(IBuffer buffer)
        {
            allBuffers.TryRemove(buffer, out _);
            memoryManager.ReleaseBytes(buffer.ReservedDataSize);
        }

        public IEnumerator<IBuffer> GetEnumerator() => allBuffers.Keys.GetEnumerator();

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

            return allBuffers.TryAdd(buffer, byte.MinValue);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}