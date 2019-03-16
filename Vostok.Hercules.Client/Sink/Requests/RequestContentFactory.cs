using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal class RequestContentFactory : IRequestContentFactory
    {
        public CompositeContent CreateContent(IReadOnlyList<BufferSnapshot> snapshots, out int recordsCount, out int recordsSize)
        {
            if (snapshots.Count == 0)
                throw new ArgumentException("Provided snapshots slice is empty.");

            var contents = new Content[snapshots.Count + 1];

            recordsCount = 0;
            recordsSize = 0;

            for (var i = 0; i < snapshots.Count; i++)
            {
                var snapshot = snapshots[i];

                recordsCount += snapshot.State.RecordsCount;
                recordsSize += snapshot.State.Length;

                contents[i + 1] = new Content(snapshot.Data);
            }

            contents[0] = CreateCountContent(recordsCount);

            return new CompositeContent(contents);
        }

        [NotNull]
        private static Content CreateCountContent(int recordsCount)
        {
            var writer = new BinaryBufferWriter(sizeof(int))
            {
                Endianness = Endianness.Big
            };

            writer.Write(recordsCount);

            return new Content(writer.FilledSegment);
        }
    }
}