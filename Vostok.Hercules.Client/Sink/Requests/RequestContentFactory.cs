using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Binary;
using Vostok.Commons.Helpers.Disposable;
using Vostok.Hercules.Client.Sink.Buffers;
using BufferPool = Vostok.Commons.Collections.BufferPool;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal class RequestContentFactory : IRequestContentFactory
    {
        private readonly BufferPool bufferPool;

        public RequestContentFactory(BufferPool bufferPool)
        {
            this.bufferPool = bufferPool;
        }

        //// CR(iloktionov): Reuse the same buffer pool with GateRequestSender. We don't actually need to pool BinaryBufferWriters here due to size known in advance.
        //private readonly UnboundedObjectPool<BinaryBufferWriter> bufferPool
        //    = new UnboundedObjectPool<BinaryBufferWriter>(() => new BinaryBufferWriter(InitialBodyBufferSize) {Endianness = Endianness.Big});

        public ValueDisposable<Content> CreateContent(IReadOnlyList<BufferSnapshot> snapshots, out int recordsCount, out int recordsSize)
        {
            if (snapshots.Count == 0)
                throw new ArgumentException("Provided snapshots slice is empty.");

            recordsCount = snapshots.Sum(s => s.State.RecordsCount);
            recordsSize = snapshots.Sum(s => s.State.Length);

            var buffer = bufferPool.Rent(sizeof(int) + recordsSize);

            try
            {
                var writer = new BinaryBufferWriter(buffer);

                writer.Write(recordsCount);

                foreach (var snapshot in snapshots)
                {
                    var data = snapshot.Data;

                    if (data.Array != null)
                        writer.WriteWithoutLength(data.Array, data.Offset, data.Count);
                }

                return new ValueDisposable<Content>(
                    new Content(writer.FilledSegment),
                    new ActionDisposable(() => bufferPool.Return(buffer)));
            }
            finally
            {
                bufferPool.Return(buffer);
            }
        }
    }
}