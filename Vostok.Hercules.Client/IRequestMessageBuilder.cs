using System;

namespace Vostok.Hercules.Client
{
    internal interface IRequestMessageBuilder
    {
        ArraySegment<byte> Message { get; }
        bool TryAppend(BufferSlice slice);
        bool IsFull { get; }
    }
}