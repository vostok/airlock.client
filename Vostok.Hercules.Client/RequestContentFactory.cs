using System;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client
{
    internal class RequestContentFactory : IBodyFormatter
    {
        public CompositeContent GetContent(ArraySegment<BufferSnapshot> snapshots, out int recordsCount)
        {
            recordsCount = 0;

            if (snapshots.Count == 0)
                return new CompositeContent(Array.Empty<Content>());

            var contents = new Content[snapshots.Count + 1];

            for (var i = 0; i < snapshots.Count; i++)
            {
                var snapshot = snapshots.Array[snapshots.Offset + i];
                recordsCount += snapshot.State.RecordsCount;
                contents[i + 1] = new Content(snapshot.Data);
            }

            var count = new byte[4];

            SetRecordsCount(count, recordsCount);

            contents[0] = new Content(count);

            return new CompositeContent(contents);
        }

        private static unsafe void SetRecordsCount(byte[] buffer, int recordsCount)
        {
            fixed (byte* b = buffer)
                *(int*)b = EndiannessConverter.Convert(recordsCount, Endianness.Big);
        }
    }
}