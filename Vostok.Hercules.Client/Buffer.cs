using System;
using System.Collections.Generic;
using System.IO;
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

        public IBinaryWriter Write(int value)
        {
            EnsureAvailableBytes(sizeof(int));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(long value)
        {
            EnsureAvailableBytes(sizeof(long));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(short value)
        {
            EnsureAvailableBytes(sizeof(short));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(double value)
        {
            EnsureAvailableBytes(sizeof(double));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(float value)
        {
            EnsureAvailableBytes(sizeof(float));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(byte value)
        {
            EnsureAvailableBytes(sizeof(byte));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(bool value)
        {
            EnsureAvailableBytes(sizeof(bool));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(string value, Encoding encoding)
        {
            EnsureAvailableBytes(encoding.GetByteCount(value));
            binaryWriter.Write(value, encoding);
            return this;
        }

        public IBinaryWriter Write(byte[] value, int offset, int length)
        {
            EnsureAvailableBytes(length);
            binaryWriter.Write(value, offset, length);
            return this;
        }

        private void EnsureAvailableBytes(int amount)
        {
            var currentLength = binaryWriter.Buffer.Length;
            var expectedLength = binaryWriter.Position + amount;

            if (currentLength >= expectedLength)
                return;

            var remainingBytes = currentLength - binaryWriter.Position;
            var reserveAmount = Math.Max(currentLength, amount - remainingBytes);

            if (!memoryManager.TryReserveBytes(reserveAmount))
                throw new InternalBufferOverflowException();
        }
    }
}