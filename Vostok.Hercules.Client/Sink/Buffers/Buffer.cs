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
        private readonly IMemoryManager memoryManager;

        private BufferStateHolder committed;
        private BufferStateHolder garbage;

        public Buffer(int bufferSize, IMemoryManager memoryManager)
        {
            writer = new BinaryBufferWriter(bufferSize)
            {
                Endianness = Endianness.Big
            };

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
            if (!TryEnsureAvailableBytes(sizeof(int)))
                return;

            writer.Write(value);
        }

        public void Write(long value)
        {
            if (!TryEnsureAvailableBytes(sizeof(long)))
                return;

            writer.Write(value);
        }

        public void Write(short value)
        {
            if (!TryEnsureAvailableBytes(sizeof(short)))
                return;

            writer.Write(value);
        }

        public void Write(double value)
        {
            if (!TryEnsureAvailableBytes(sizeof(double)))
                return;

            writer.Write(value);
        }

        public void Write(float value)
        {
            if (!TryEnsureAvailableBytes(sizeof(float)))
                return;

            writer.Write(value);
        }

        public void Write(byte value)
        {
            if (!TryEnsureAvailableBytes(sizeof(byte)))
                return;

            writer.Write(value);
        }

        public void Write(bool value)
        {
            if (!TryEnsureAvailableBytes(sizeof(bool)))
                return;

            writer.Write(value);
        }

        public void Write(ushort value)
        {
            if (!TryEnsureAvailableBytes(sizeof(ushort)))
                return;

            writer.Write(value);
        }

        public void Write(Guid value)
        {
            const int size = 16;

            if (!TryEnsureAvailableBytes(size))
                return;

            writer.Write(value);
        }

        public void WriteWithoutLength(string value)
        {
            if (!TryEnsureAvailableBytes(Encoding.UTF8.GetMaxByteCount(value.Length)))
                return;

            writer.WriteWithoutLength(value);
        }

        public void WriteWithLength(string value)
        {
            if (!TryEnsureAvailableBytes(sizeof(int) + Encoding.UTF8.GetMaxByteCount(value.Length)))
                return;

            writer.WriteWithLength(value);
        }

        public void WriteWithLength(byte[] value, int offset, int length)
        {
            if (!TryEnsureAvailableBytes(sizeof(int) + length))
                return;

            writer.WriteWithLength(value, offset, length);
        }

        public void WriteWithoutLength(byte[] value, int offset, int length)
        {
            if (!TryEnsureAvailableBytes(length))
                return;

            writer.WriteWithoutLength(value, offset, length);
        }

        public int WriteVarlen(uint value)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!TryEnsureAvailableBytes(sizeof(uint) + 1))
                return 0;

            return writer.WriteVarlen(value);
        }

        public int WriteVarlen(ulong value)
        {
            // ReSharper disable once ConvertIfStatementToReturnStatement
            if (!TryEnsureAvailableBytes(sizeof(ulong) + 1))
                return 0;

            return writer.WriteVarlen(value);
        }

        public void WriteWithLength(string value, Encoding encoding)
        {
            if (!TryEnsureAvailableBytes(sizeof(int) + encoding.GetMaxByteCount(value.Length)))
                return;

            writer.WriteWithLength(value);
        }

        public void WriteWithoutLength(string value, Encoding encoding)
        {
            if (!TryEnsureAvailableBytes(encoding.GetMaxByteCount(value.Length)))
                return;

            writer.WriteWithoutLength(value);
        }

        public void WriteWithLength(byte[] value)
        {
            if (!TryEnsureAvailableBytes(sizeof(int) + value.Length))
                return;

            writer.WriteWithLength(value);
        }

        public void Write(uint value)
        {
            if (!TryEnsureAvailableBytes(sizeof(uint)))
                return;

            writer.Write(value);
        }

        public void WriteWithoutLength(byte[] value)
        {
            if (!TryEnsureAvailableBytes(value.Length))
                return;

            writer.WriteWithoutLength(value);
        }

        public void Write(ulong value)
        {
            if (!TryEnsureAvailableBytes(sizeof(ulong)))
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

        private bool TryEnsureAvailableBytes(int amount)
        {
            if (IsOverflowed)
                return false;

            var currentLength = writer.Buffer.Length;
            var maxLengthAfterWrite = writer.Position + amount;

            if (currentLength >= maxLengthAfterWrite)
                return true;

            var remainingBytes = currentLength - writer.Position;
            var reserveAmount = Math.Max(currentLength, amount - remainingBytes);

            if (!memoryManager.TryReserveBytes(reserveAmount))
            {
                IsOverflowed = true;
                return false;
            }

            return true;
        }
    }
}