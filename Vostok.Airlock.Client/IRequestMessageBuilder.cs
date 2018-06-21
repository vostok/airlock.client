using System;

namespace Vostok.Airlock.Client
{
    internal interface IRequestMessageBuilder
    {
        bool TryAppend(BufferSlice slice);
        ArraySegment<byte> Message { get; }
    }
}   