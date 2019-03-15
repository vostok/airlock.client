using System.Threading;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    internal class BufferStateHolder
    {
        public BufferStateHolder()
        {
            Value = default;
        }

        private long state;

        public unsafe BufferState Value
        {
            get
            {
                var val = Interlocked.Read(ref state);
                return *(BufferState*)&val;
            }
            set => Interlocked.Exchange(ref state, *(long*)&value);
        }
    }
}