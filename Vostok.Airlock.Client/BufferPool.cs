using System.Collections.Concurrent;
using Vostok.Commons.Binary;

namespace Vostok.Airlock.Client
{
    internal class BufferPool : IBufferPool
    {
        private readonly IMemoryManager memoryManager;
        private readonly int initialBufferSize;
        private readonly ConcurrentQueue<IBuffer> buffers;

        public BufferPool(IMemoryManager memoryManager, int initialCount, int initialBufferSize)
        {
            this.memoryManager = memoryManager;
            this.initialBufferSize = initialBufferSize;

            buffers = new ConcurrentQueue<IBuffer>();

            for (var i = 0; i < initialCount; i++)
            {
                if (TryCreateBuffer(out var buffer))
                {
                    buffers.Enqueue(buffer);
                }
                else break;
            }
        }

        public bool TryAcquire(out IBuffer buffer)
        {
            return buffers.TryDequeue(out buffer) || TryCreateBuffer(out buffer);
        }

        public void Release(IBuffer buffer)
        {
            buffers.Enqueue(buffer);
        }

        private bool TryCreateBuffer(out IBuffer buffer)
        {
            if (!memoryManager.TryReserveBytes(initialBufferSize))
            {
                buffer = null;
                return false;
            }

            var binaryWriter = new BinaryBufferWriter(new byte[initialBufferSize]);

            buffer = new Buffer(binaryWriter, memoryManager);
            return true;
        }
    }
}