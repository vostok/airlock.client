using System;
using System.Linq;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client
{
    internal class BodyFormatter : IBodyFormatter
    {
        private readonly BinaryBufferWriter writer;

        public BodyFormatter(int maximumBatchSize) =>
            writer = new BinaryBufferWriter(maximumBatchSize) {Endianness = Endianness.Big};

        public ArraySegment<byte> GetContent(ArraySegment<BufferSnapshot> snapshots, out int recordsCount)
        {
            if (snapshots.Count == 0)
            {
                recordsCount = 0;
                return new ArraySegment<byte>(Array.Empty<byte>());
            }

            if (snapshots.Count == 1)
            {
                var snapshot = snapshots.Array[snapshots.Offset];

                recordsCount = snapshot.State.RecordsCount;
                SetRecordsCount(snapshot.Buffer, recordsCount);

                return snapshot.Data;
            }

            recordsCount = snapshots.Sum(x => x.State.RecordsCount);

            writer.Position = 0;
            writer.Write(recordsCount);

            foreach (var snapshot in snapshots)
                writer.WriteWithoutLength(snapshot.Buffer, Buffer.InitialPosition, snapshot.State.LengthOfRecords);

            return writer.FilledSegment;
        }

        private static unsafe void SetRecordsCount(byte[] buffer, int recordsCount)
        {
            if (buffer.Length < sizeof(int))
                throw new ArgumentException($"Buffer length {buffer.Length} is less than {sizeof(int)}.");

            fixed (byte* b = buffer)
                *(int*)b = EndiannessConverter.Convert(recordsCount, Endianness.Big);
        }
    }
}