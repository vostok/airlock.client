using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class Buffer : IBuffer, IBinaryWriter
    {
        private readonly BinaryBufferWriter binaryWriter;
        private readonly IMemoryManager memoryManager;
        private readonly List<BufferGarbageSegment> garbage;

        private volatile Dictionary<int, int> records;

        public Buffer(byte[] buffer, IMemoryManager memoryManager)
        {
            binaryWriter = new BinaryBufferWriter(buffer);
            this.memoryManager = memoryManager;
            garbage = new List<BufferGarbageSegment>();
            records = new Dictionary<int, int>();
        }

        public IBinaryWriter BeginRecord() => this;

        public void Commit(int recordSize) => records.Add(binaryWriter.Position - recordSize, recordSize);

        public int GetRecordSize(int offset) => records[offset];

        public int EstimateRecordsCountForMonitoring() => records.Count;

        public bool IsEmpty() => binaryWriter.Position == 0;

        public BufferSnapshot MakeSnapshot() =>
            new BufferSnapshot(this, binaryWriter.Buffer, binaryWriter.Position, records.Count);

        public void RequestGarbageCollection(int offset, int length, int recordsCount) =>
            garbage.Add(new BufferGarbageSegment {Offset = offset, Length = length, RecordsCount = recordsCount});

        public void CollectGarbage()
        {
            if (garbage.Count == 0)
                return;

            if (binaryWriter.Position > 0)
            {
                garbage.Sort((x, y) => x.Offset.CompareTo(y.Offset));

                var usefulBytesEndingPosition = DefragmentationManager.Run(binaryWriter.FilledSegment, garbage);

                binaryWriter.Position = usefulBytesEndingPosition;

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
            
            return records
                .Where(x => !garbageRecords.ContainsKey(x.Key))
                .ToDictionary(x => GetNewOffsetForRecord(x.Key), x => x.Value);
        }

        public int Position
        {
            get => binaryWriter.Position;
            set => binaryWriter.Position = value;
        }
        
        public bool IsOverflowed { get; set; }

        public IBinaryWriter Write(int value)
        {
            if (!TryEnsureAvailableBytes(sizeof(int)))
                return this;
            
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(long value)
        {
            if (!TryEnsureAvailableBytes(sizeof(long)))
                return this;
            
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(short value)
        {
            if (!TryEnsureAvailableBytes(sizeof(short)))
                return this;
            
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(double value)
        {
            if (!TryEnsureAvailableBytes(sizeof(double)))
                return this;
            
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(float value)
        {
            if (!TryEnsureAvailableBytes(sizeof(float)))
                return this;
            
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(byte value)
        {
            if (!TryEnsureAvailableBytes(sizeof(byte)))
                return this;
            
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(bool value)
        {
            if (!TryEnsureAvailableBytes(sizeof(bool)))
                return this;
            
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(string value, Encoding encoding)
        {
            if (!TryEnsureAvailableBytes(encoding.GetByteCount(value)))
                return this;
            
            binaryWriter.Write(value, encoding);
            return this;
        }

        public IBinaryWriter Write(byte[] value, int offset, int length)
        {
            if (!TryEnsureAvailableBytes(length))
                return this;

            binaryWriter.Write(value, offset, length);
            return this;
        }

        private bool TryEnsureAvailableBytes(int amount)
        {
            if (IsOverflowed)
                return false;
            
            var currentLength = binaryWriter.Buffer.Length;
            var expectedLength = binaryWriter.Position + amount;

            if (currentLength >= expectedLength)
                return true;

            var remainingBytes = currentLength - binaryWriter.Position;
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