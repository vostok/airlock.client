using System;
using System.Text;
using Vostok.Clusterclient.Core.Modules;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class Buffer : IBuffer, IHerculesBinaryWriter
    {
        private const int InitialPosition = 4;
        
        private readonly IHerculesBinaryWriter writer;
        private readonly IMemoryManager memoryManager;
        private readonly AtomicBoolean isLocked = new AtomicBoolean(false);

        private BufferStateHolder committed;
        private BufferStateHolder garbage;

        public Buffer(int bufferSize, IMemoryManager memoryManager)
        {
            writer = new HerculesBinaryWriter(bufferSize);

            this.memoryManager = memoryManager;
            
            writer.Write(0);
            committed.Value = new BufferState(InitialPosition, 0);
        }

        public IHerculesBinaryWriter BeginRecord() => this;

        public void Commit(int recordSize)
        {
            var oldValue = committed.Value;
            committed.Value = new BufferState(oldValue.Length + recordSize, oldValue.RecordsCount + 1);
        }

        public int EstimateRecordsCountForMonitoring() => committed.Value.RecordsCount;

        public bool IsEmpty() => writer.Position == 0;

        public BufferSnapshot MakeSnapshot()
        {
            return new BufferSnapshot(this, writer.Array, committed.Value);
        }

        public void RequestGarbageCollection(BufferState state)
        {
            garbage.Value = state;
        }

        public bool TryLock() =>
            isLocked.TrySetTrue();

        public void Unlock() =>
            isLocked.Value = false;

        public bool HasGarbage() =>
            garbage.Value.RecordsCount != 0;

        /// <summary>
        /// <threadsafety>This method is NOT threadsafe and should be called only from <see cref="BufferPool.TryAcquire"/> and <see cref="BufferPool.MakeSnapshot"/>.</threadsafety>
        /// </summary>
        public void CollectGarbage()
        {
            var garbageState = garbage.Value;
            if (garbageState.Length == 0)
                return;
         
            System.Buffer.BlockCopy(
                writer.Array, garbageState.Length,
                writer.Array, InitialPosition, writer.Position - garbageState.Length);

            writer.Position -= garbageState.Length - InitialPosition;
            committed.Value -= garbageState - new BufferState(InitialPosition, 0);
            garbage.Value = new BufferState();
        }

        public int Position
        {
            get => writer.Position;
            set => writer.Position = value;
        }
        
        public bool IsOverflowed { get; set; }
        public byte[] Array => writer.Array;
        public ArraySegment<byte> FilledSegment => writer.FilledSegment;
        public Encoding Encoding => writer.Encoding;

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
            if (!TryEnsureAvailableBytes(writer.Encoding.GetMaxByteCount(value.Length)))
                return;
            
            writer.WriteWithoutLength(value);
        }

        public void WriteWithLength(string value)
        {
            if (!TryEnsureAvailableBytes(sizeof(int) + writer.Encoding.GetMaxByteCount(value.Length)))
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

        private bool TryEnsureAvailableBytes(int amount)
        {
            if (IsOverflowed)
                return false;
            
            var currentLength = writer.Array.Length;
            var expectedLength = writer.Position + amount;

            if (currentLength >= expectedLength)
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