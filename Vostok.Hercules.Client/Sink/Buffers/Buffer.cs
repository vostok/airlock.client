using System;
using Vostok.Commons.Binary;
using Vostok.Commons.Threading;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal partial class Buffer : IBuffer
    {
        private readonly BinaryBufferWriter writer;
        private readonly IMemoryManager memoryManager;
        private readonly int maxSize;

        private readonly BufferStateHolder committed = new BufferStateHolder();
        private readonly BufferStateHolder garbage = new BufferStateHolder();
        private readonly AtomicBoolean isLocked = new AtomicBoolean(false);

        public Buffer(int initialSize, int maxSize, IMemoryManager memoryManager)
        {
            writer = new BinaryBufferWriter(initialSize)
            {
                Endianness = Endianness.Big
            };

            this.maxSize = maxSize;
            this.memoryManager = memoryManager;
        }

        public bool IsOverflowed { get; set; }

        public int Capacity => writer.Buffer.Length;

        public ArraySegment<byte> FilledSegment => writer.FilledSegment;

        public BufferState Committed => committed.Value;

        public BufferState Garbage => garbage.Value;

        public long UsefulDataSize => Committed.Length - Garbage.Length;

        public void CommitRecord(int size)
            => committed.Value += new BufferState(size, 1);

        public void ReportGarbage(BufferState region)
            => garbage.Value = region;

        public bool TryLock()
            => isLocked.TrySetTrue();

        public void Unlock()
            => isLocked.Value = false;

        public BufferSnapshot TryMakeSnapshot()
        {
            if (!TryCollectGarbage())
                return null;

            // (epeshk): we should read committed.Value BEFORE acquiring a buffer because buffer may be changed on resizing.
            var committedState = Committed;
            var internalBuffer = writer.Buffer;

            return new BufferSnapshot(this, committedState, internalBuffer);
        }

        public bool TryCollectGarbage()
        {
            if (Garbage.Length == 0)
                return true;

            if (!isLocked.TrySetTrue())
                return false;

            try
            {
                var garbageState = Garbage;
                if (garbageState.Length == 0)
                    return true;

                System.Buffer.BlockCopy(
                    writer.Buffer,
                    garbageState.Length,
                    writer.Buffer,
                    0,
                    (int)writer.Position - garbageState.Length);

                writer.Position -= garbageState.Length;
                committed.Value -= garbageState;

                // (epeshk): reset garbage state last for synchronization with TryMakeSnapshot:
                garbage.Value = default;
                return true;
            }
            finally
            {
                isLocked.Value = false;
            }
        }
    }
}
