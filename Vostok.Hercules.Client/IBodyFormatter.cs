using System;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hercules.Client
{
    internal interface IBodyFormatter
    {
        CompositeContent GetContent(ArraySegment<BufferSnapshot> snapshots, out int recordsCount);
    }
}