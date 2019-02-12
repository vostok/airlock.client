using System.Threading;

namespace Vostok.Hercules.Client
{
    internal struct BufferStateHolder
    {
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