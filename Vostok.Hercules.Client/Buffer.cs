using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class Buffer : IBuffer, IHerculesBinaryWriter
    {
        private readonly IHerculesBinaryWriter writer;
        private readonly IMemoryManager memoryManager;
        private readonly List<BufferGarbageSegment> garbage;
        private readonly object sync = new object();

        private volatile Dictionary<int, int> records;

        public Buffer(int bufferSize, IMemoryManager memoryManager)
        {
            writer = new HerculesBinaryWriter(bufferSize);
            this.memoryManager = memoryManager;
            garbage = new List<BufferGarbageSegment>();
            records = new Dictionary<int, int>();
        }

        public IHerculesBinaryWriter BeginRecord() => this;

        public void Commit(int recordSize)
        {
            lock (sync)
                records.Add(writer.Position - recordSize, recordSize);
        }

        public int GetRecordSize(int offset)
        {
            lock (sync)
                return records[offset];
        }

        public int EstimateRecordsCountForMonitoring() => records.Count;

        public bool IsEmpty() => writer.Position == 0;

        public BufferSnapshot MakeSnapshot()
        {
            lock (sync)
                return new BufferSnapshot(this, writer.Array, writer.Position, records.Count);
        }

        public void RequestGarbageCollection(int offset, int length, int recordsCount) =>
            garbage.Add(new BufferGarbageSegment {Offset = offset, Length = length, RecordsCount = recordsCount});

        /// <summary>
        /// <threadsafety>This method is NOT threadsafe and should be called only from <see cref="BufferPool.TryAcquire"/> and <see cref="BufferPool.MakeSnapshot"/>.</threadsafety>
        /// </summary>
        public void CollectGarbage()
        {
            if (garbage.Count == 0)
                return;

            if (writer.Position > 0)
            {
                garbage.Sort((x, y) => x.Offset.CompareTo(y.Offset));

                var usefulBytesEndingPosition = DefragmentationManager.Run(writer.FilledSegment, garbage);

                writer.Position = usefulBytesEndingPosition;

                if (records.Count > 0)
                {
                    var garbageRecordsCount = garbage.Sum(x => x.RecordsCount);

                    records = RemoveGarbageRecords(garbageRecordsCount);
                }
            }

            garbage.Clear();
        }

        private Dictionary<int, int> RemoveGarbageRecords(int garbageRecordsCount)
        {
            var garbageRecords = new Dictionary<int, byte>(garbageRecordsCount);

            foreach (var garbageSegment in garbage)
            {
                var currentOffset = garbageSegment.Offset;
                for (var i = 0; i < garbageSegment.RecordsCount; i++)
                {
                    garbageRecords.Add(currentOffset, 0);
                    currentOffset += GetRecordSize(currentOffset);
                }
            }
            
            int GetNewOffsetForRecord(int oldOffset)
            {
                //TODO: use binary search
                return oldOffset - garbage.Where(x => x.Offset < oldOffset).Sum(x => x.Length);
            }
            
            lock (sync)
                return records
                    .Where(x => !garbageRecords.ContainsKey(x.Key))
                    .ToDictionary(x => GetNewOffsetForRecord(x.Key), x => x.Value);
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