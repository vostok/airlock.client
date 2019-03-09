using System;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Sink.Buffers;

namespace Vostok.Hercules.Client.Sink.Requests
{
    internal interface IRequestContentFactory
    {
        CompositeContent CreateContent(ArraySegment<BufferSnapshot> snapshots, out int recordsCount);
    }
}