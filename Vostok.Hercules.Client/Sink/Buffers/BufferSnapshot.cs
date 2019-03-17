using System;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal class BufferSnapshot
    {
        private readonly byte[] buffer;

        public BufferSnapshot(IBuffer source, BufferState state, byte[] buffer)
        {
            Source = source;
            State = state;

            this.buffer = buffer;
        }

        public IBuffer Source { get; }

        public BufferState State { get; }

        public ArraySegment<byte> Data => new ArraySegment<byte>(buffer, 0, State.Length);
    }
}