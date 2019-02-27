using System;

namespace Vostok.Hercules.Client
{
    internal interface IBodyFormatter
    {
        ArraySegment<byte> GetContent(ArraySegment<BufferSnapshot> snapshots, out int recordsCount);
    }
}