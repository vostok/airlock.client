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

        public BufferState Committed => committed.Value;

        public BufferState Garbage => garbage.Value;

        public long UsefulDataSize => Committed.Length - Garbage.Length;

        public long ReservedDataSize => writer.Buffer.Length;

        public void CommitRecord(int size)
        {
            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), $"Attempt to commit a record of incorrect size {size}.");

            var currentCommitted = committed.Value;
            if (currentCommitted.Length + size > Position)
                throw new InvalidOperationException($"Attempt to commit a record of size {size} on top of current commited length {currentCommitted.Length} past current physical length {Position}.");

            committed.Value = currentCommitted + new BufferState(size, 1);
        }

        public void ReportGarbage(BufferState region)
        {
            var currentCommitted = committed.Value;
            var currentGarbage = garbage.Value;

            if (!currentGarbage.IsEmpty)
                throw new InvalidOperationException($"Attempt to report a garbage region of size {region.Length} when there's already garbage of size {currentGarbage.Length}.");

            if (region.Length > currentCommitted.Length)
                throw new InvalidOperationException($"Attempt to report a garbage region of size {region.Length} that exceeds current committed region of size {currentCommitted.Length}.");

            if (region.RecordsCount > currentCommitted.RecordsCount)
                throw new InvalidOperationException($"Attempt to report a garbage region with {region.RecordsCount} records, which is more than current committed records count {currentCommitted.RecordsCount}.");

            garbage.Value = region;
        }

        public bool TryLock()
            => isLocked.TrySetTrue();

        public void Unlock()
            => isLocked.Value = false;

        public BufferSnapshot TryMakeSnapshot()
        {
            if (!TryCollectGarbage())
                return null;

            // (epeshk): we should read committed.Value BEFORE acquiring a buffer because buffer may be changed due to resizing.
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
                CollectGarbage();
                return true;
            }
            finally
            {
                isLocked.Value = false;
            }
        }

        public void CollectGarbage()
        {
            var garbageState = Garbage;
            if (garbageState.Length == 0)
                return;

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
        }
    }
}