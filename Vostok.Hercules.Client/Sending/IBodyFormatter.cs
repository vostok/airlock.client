using System;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Sink;

namespace Vostok.Hercules.Client.Sending
{
    internal interface IBodyFormatter
    {
        CompositeContent CreateContent(ArraySegment<BufferSnapshot> snapshots, out int recordsCount);
    }
}