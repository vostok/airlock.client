using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vostok.Commons.Binary;

namespace Vostok.Airlock.Client
{
    internal class Buffer : IBuffer, IBinaryWriter
    {
        private readonly BinaryBufferWriter binaryWriter;
        private readonly IMemoryManager memoryManager;
        private readonly List<BufferGarbageSegment> garbage;

        private volatile BufferSnapshot snapshot;
        private volatile int recordsCounter;

        public Buffer(byte[] buffer, IMemoryManager memoryManager)
        {
            binaryWriter = new BinaryBufferWriter(buffer);
            this.memoryManager = memoryManager;
            garbage = new List<BufferGarbageSegment>();
        }

        public int Position
        {
            get => binaryWriter.Position;
            set => binaryWriter.Position = value;
        }

        public IBinaryWriter BeginRecord()
        {
            return this;
        }

        public void Commit()
        {
            ++recordsCounter;
        }

        public BufferSnapshot MakeSnapshot()
        {
            return snapshot = new BufferSnapshot(this, binaryWriter.Buffer, Position, recordsCounter);
        }

        public bool IsEmpty()
        {
            return Position == 0;
        }

        public void RequestGarbageCollection(int offset, int length, int recordsCount)
        {
            garbage.Add(new BufferGarbageSegment {Offset = offset, Length = length, RecordsCount = recordsCount});
        }

        public void CollectGarbage()
        {
            if (garbage.Count == 0)
            {
                return;
            }

            if (snapshot.Position > 0)
            {
                garbage.Sort((x, y) => x.Offset.CompareTo(y.Offset));

                garbage.Join();

                var usefulBytesEndingPosition = DefragmentationManager.Run(binaryWriter.Buffer, garbage);

                if (snapshot.Position != Position)
                {
                    var bytesWrittenAfter = Position - snapshot.Position;
                    System.Buffer.BlockCopy(binaryWriter.Buffer, snapshot.Position, binaryWriter.Buffer, usefulBytesEndingPosition, bytesWrittenAfter);
                    Position = usefulBytesEndingPosition + bytesWrittenAfter;
                }
                else
                {
                    Position = usefulBytesEndingPosition;
                }
            }

            if (snapshot.RecordsCount > 0)
            {
                recordsCounter -= garbage.Sum(x => x.RecordsCount);
            }

            garbage.Clear();
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

        public IBinaryWriter Write(uint value)
        {
            EnsureAvailableBytes(sizeof(uint));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(ulong value)
        {
            EnsureAvailableBytes(sizeof(ulong));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(ushort value)
        {
            EnsureAvailableBytes(sizeof(ushort));
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

        public IBinaryWriter Write(float value)
        {
            EnsureAvailableBytes(sizeof(float));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(double value)
        {
            EnsureAvailableBytes(sizeof(double));
            binaryWriter.Write(value);
            return this;
        }

        public unsafe IBinaryWriter Write(Guid value)
        {
            EnsureAvailableBytes(sizeof(Guid));
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(string value, Encoding encoding)
        {
            EnsureAvailableBytes(sizeof(int) + encoding.GetMaxByteCount(value.Length));
            binaryWriter.Write(value, encoding);
            return this;
        }

        public IBinaryWriter WriteWithoutLengthPrefix(string value, Encoding encoding)
        {
            EnsureAvailableBytes(encoding.GetMaxByteCount(value.Length));
            binaryWriter.WriteWithoutLengthPrefix(value, encoding);
            return this;
        }

        public IBinaryWriter Write(byte[] value)
        {
            EnsureAvailableBytes(sizeof(int) + value.Length);
            binaryWriter.Write(value);
            return this;
        }

        public IBinaryWriter Write(byte[] value, int offset, int length)
        {
            EnsureAvailableBytes(sizeof(int) + length);
            binaryWriter.Write(value, offset, length);
            return this;
        }

        public IBinaryWriter WriteWithoutLengthPrefix(byte[] value)
        {
            EnsureAvailableBytes(value.Length);
            binaryWriter.WriteWithoutLengthPrefix(value);
            return this;
        }

        public IBinaryWriter WriteWithoutLengthPrefix(byte[] value, int offset, int length)
        {
            EnsureAvailableBytes(length);
            binaryWriter.WriteWithoutLengthPrefix(value, offset, length);
            return this;
        }

        private void EnsureAvailableBytes(int amount)
        {
            var currentLength = binaryWriter.Buffer.Length;
            var expectedLength = binaryWriter.Position + amount;

            if (currentLength >= expectedLength)
            {
                return;
            }

            var remainingBytes = currentLength - binaryWriter.Position;
            var reserveAmount = Math.Max(currentLength, amount - remainingBytes);

            if (!memoryManager.TryReserveBytes(reserveAmount))
            {
                throw new InternalBufferOverflowException();
            }
        }
    }
}