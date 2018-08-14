using System;

namespace Vostok.Airlock.Client
{
    internal interface IRequestMessageBuilder
    {
        ArraySegment<byte> Message { get; }
        bool TryAppend(BufferSlice slice);
    }
}