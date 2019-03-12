using System;
using System.Text;
using Vostok.Commons.Binary;
using Vostok.Commons.Threading;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal class Buffer : IBuffer
    {
        private readonly AtomicBoolean isLockedForWrite = new AtomicBoolean(false);

        private readonly BinaryBufferWriter writer;
        private readonly int maxSize;
        private readonly IMemoryManager memoryManager;

        private readonly BufferStateHolder committed = new BufferStateHolder();
        private readonly BufferStateHolder garbage = new BufferStateHolder();

        public Buffer(int bufferSize, int maxSize, IMemoryManager memoryManager)
        {
            writer = new BinaryBufferWriter(bufferSize)
            {
                Endianness = Endianness.Big
            };

            this.maxSize = maxSize;
            this.memoryManager = memoryManager;

            committed.Value = default;
        }

        public long Position
        {
            get => writer.Position;
            set => writer.Position = value;
        }

        public Endianness Endianness
        {
            get => Endianness.Big;
            set => throw new NotSupportedException();
        }

        public bool IsOverflowed { get; set; }
        public byte[] Array => writer.Buffer;
        public ArraySegment<byte> FilledSegment => writer.FilledSegment;

        public void Commit(int recordSize)
        {
            var oldValue = committed.Value;
            committed.Value = new BufferState(oldValue.Length + recordSize, oldValue.RecordsCount + 1);
        }

        public BufferState GetState() => committed.Value;
        public long GetUsefulLength() => committed.Value.Length - garbage.Value.Length;

        public BufferSnapshot TryMakeSnapshot()
        {
            return TryCollectGarbage()
                ? new BufferSnapshot(this, writer.Buffer, committed.Value)
                : null;
        }

        public void RequestGarbageCollection(BufferState state)
        {
            garbage.Value = state;
        }

        public bool TryLock() => isLockedForWrite.TrySetTrue();
        public void Unlock() => isLockedForWrite.Value = false;

        /// <summary>
        /// <threadsafety>This method is NOT threadsafe and should be called only when buffer is not available for write.</threadsafety>
        /// </summary>
        public void CollectGarbage()
        {
            var garbageState = garbage.Value;
            if (garbageState.Length == 0)
                return;

            System.Buffer.BlockCopy(
                writer.Buffer,
                garbageState.Length,
                writer.Buffer,
                0,
                (int)writer.Position - garbageState.Length);

            garbage.Value = default;
            writer.Position -= garbageState.Length;
            committed.Value -= garbageState;
        }

        public void Write(int value)
        {
            if (!EnsureAvailableBytes(sizeof(int)))
                return;

            writer.Write(value);
        }

        public void Write(long value)
        {
            if (!EnsureAvailableBytes(sizeof(long)))
                return;

            writer.Write(value);
        }

        public void Write(short value)
        {
            if (!EnsureAvailableBytes(sizeof(short)))
                return;

            writer.Write(value);
        }

        public void Write(double value)
        {
            if (!EnsureAvailableBytes(sizeof(double)))
                return;

            writer.Write(value);
        }

        public void Write(float value)
        {
            if (!EnsureAvailableBytes(sizeof(float)))
                return;

            writer.Write(value);
        }

        public void Write(byte value)
        {
            if (!EnsureAvailableBytes(sizeof(byte)))
                return;

            writer.Write(value);
        }

        public void Write(bool value)
        {
            if (!EnsureAvailableBytes(sizeof(bool)))
                return;

            writer.Write(value);
        }

        public void Write(ushort value)
        {
            if (!EnsureAvailableBytes(sizeof(ushort)))
                return;

            writer.Write(value);
        }

        public void Write(Guid value)
        {
            const int size = 16;

            if (!EnsureAvailableBytes(size))
                return;

            writer.Write(value);
        }

        public void WriteWithoutLength(string value)
        {
            if (!EnsureAvailableBytes(Encoding.UTF8.GetMaxByteCount(value.Length)))
                return;

            writer.WriteWithoutLength(value);
        }

        public void WriteWithLength(string value)
        {
            if (!EnsureAvailableBytes(sizeof(int) + Encoding.UTF8.GetMaxByteCount(value.Length)))
                return;

            writer.WriteWithLength(value);
        }

        public void WriteWithLength(byte[] value, int offset, int length)
        {
            if (!EnsureAvailableBytes(sizeof(int) + length))
                return;

            writer.WriteWithLength(value, offset, length);
        }

        public void WriteWithoutLength(byte[] value, int offset, int length)
        {
            if (!EnsureAvailableBytes(length))
                return;

            writer.WriteWithoutLength(value, offset, length);
        }

        public int WriteVarlen(uint value)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!EnsureAvailableBytes(sizeof(uint) + 1))
                return 0;

            return writer.WriteVarlen(value);
        }

        public int WriteVarlen(ulong value)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!EnsureAvailableBytes(sizeof(ulong) + 1))
                return 0;

            return writer.WriteVarlen(value);
        }

        public void WriteWithLength(string value, Encoding encoding)
        {
            if (!EnsureAvailableBytes(sizeof(int) + encoding.GetMaxByteCount(value.Length)))
                return;

            writer.WriteWithLength(value);
        }

        public void WriteWithoutLength(string value, Encoding encoding)
        {
            if (!EnsureAvailableBytes(encoding.GetMaxByteCount(value.Length)))
                return;

            writer.WriteWithoutLength(value);
        }

        public void WriteWithLength(byte[] value)
        {
            if (!EnsureAvailableBytes(sizeof(int) + value.Length))
                return;

            writer.WriteWithLength(value);
        }

        public void Write(uint value)
        {
            if (!EnsureAvailableBytes(sizeof(uint)))
                return;

            writer.Write(value);
        }

        public void WriteWithoutLength(byte[] value)
        {
            if (!EnsureAvailableBytes(value.Length))
                return;

            writer.WriteWithoutLength(value);
        }

        public void Write(ulong value)
        {
            if (!EnsureAvailableBytes(sizeof(ulong)))
                return;

            writer.Write(value);
        }

        private bool TryCollectGarbage()
        {
            if (garbage.Value.Length == 0)
                return true;

            if (isLockedForWrite.TrySetTrue())
            {
                CollectGarbage();
                isLockedForWrite.Value = false;
                return true;
            }

            return false;
        }

        private bool EnsureAvailableBytes(int amount)
        {
            if (TryEnsureAvailableBytes(amount))
                return true;

            IsOverflowed = true;
            return false;
        }

        private bool TryEnsureAvailableBytes(int amount)
        {
            if (IsOverflowed)
                return false;

            var currentLength = writer.Buffer.Length;
            var maxPositionAfterWrite = writer.Position + amount;

            if (currentLength >= maxPositionAfterWrite)
                return true;

            if (maxPositionAfterWrite > maxSize)
                return false;

            if (currentLength + currentLength > maxSize)
                return TryResize(maxSize) || TryResize((int) maxPositionAfterWrite);

            var reserveAmount = Math.Max(currentLength, maxPositionAfterWrite - currentLength);

            return memoryManager.TryReserveBytes(reserveAmount);
        }

        private bool TryResize(int capacity)
        {
            if (!memoryManager.TryReserveBytes(capacity - writer.Buffer.Length))
                return false;

            writer.Resize(capacity);
            return true;
        }
    }
}