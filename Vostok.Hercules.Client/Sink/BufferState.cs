using System.Runtime.InteropServices;

namespace Vostok.Hercules.Client.Sink
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct BufferState
    {
        public readonly int Length;
        public readonly int RecordsCount;

        public BufferState(int length, int recordsCount)
        {
            Length = length;
            RecordsCount = recordsCount;
        }

        public static BufferState operator+(BufferState a, BufferState b)
            => new BufferState(a.Length + b.Length, a.RecordsCount + b.RecordsCount);

        public static BufferState operator-(BufferState a, BufferState b)
            => new BufferState(a.Length - b.Length, a.RecordsCount - b.RecordsCount);
    }
}