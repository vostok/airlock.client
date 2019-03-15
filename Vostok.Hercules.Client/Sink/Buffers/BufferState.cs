using System;
using System.Runtime.InteropServices;

namespace Vostok.Hercules.Client.Sink.Buffers
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct BufferState : IEquatable<BufferState>
    {
        public readonly int Length;
        public readonly int RecordsCount;

        public BufferState(int length, int recordsCount)
        {
            Length = length;
            RecordsCount = recordsCount;
        }

        public bool IsEmpty => Length == 0 && RecordsCount == 0;

        public static BufferState operator+(BufferState a, BufferState b)
            => new BufferState(a.Length + b.Length, a.RecordsCount + b.RecordsCount);

        public static BufferState operator-(BufferState a, BufferState b)
            => new BufferState(a.Length - b.Length, a.RecordsCount - b.RecordsCount);

        #region Equality

        public bool Equals(BufferState other) =>
          Length == other.Length && RecordsCount == other.RecordsCount;

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            return obj is BufferState other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Length * 397) ^ RecordsCount;
            }
        } 

        #endregion
    }
}