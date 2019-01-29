using System;

namespace Vostok.Hercules.Client
{
    internal class BufferSnapshot
    {
        public BufferSnapshot(IBuffer parent, byte[] buffer, BufferState state)
        {
            Parent = parent;
            Buffer = buffer;
            State = state;
        }

        public IBuffer Parent { get; }
        public byte[] Buffer { get; }
        public BufferState State { get; }
        public ArraySegment<byte> Data => new ArraySegment<byte>(Buffer, 0, State.Length);
    }
}