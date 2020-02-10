using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Binary;
using Vostok.Commons.Collections;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal class RequestContentFactory : IRequestContentFactory
    {
        private const int InitialBodyBufferSize = 4096;

        private readonly UnboundedObjectPool<BinaryBufferWriter> bufferPool
            = new UnboundedObjectPool<BinaryBufferWriter>(() => new BinaryBufferWriter(InitialBodyBufferSize) {Endianness = Endianness.Big});

        public ValueDisposable<Content> CreateContent(IReadOnlyList<BufferSnapshot> snapshots, out int recordsCount, out int recordsSize)
        {
            if (snapshots.Count == 0)
                throw new ArgumentException("Provided snapshots slice is empty.");

            recordsCount = snapshots.Sum(s => s.State.RecordsCount);
            recordsSize = snapshots.Sum(s => s.State.Length);

            var disposable = bufferPool.Acquire(out var writer);

            writer.Reset();
            writer.Write(recordsCount);

            foreach (var snapshot in snapshots)
            {
                var data = snapshot.Data;

                if (data.Array != null)
                    writer.WriteWithoutLength(data.Array, data.Offset, data.Count);
            }

            return new ValueDisposable<Content>(new Content(writer.FilledSegment), disposable);
        }
    }
}