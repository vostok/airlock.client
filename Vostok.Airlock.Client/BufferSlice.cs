namespace Vostok.Airlock.Client
{
    internal class BufferSlice
    {
        public BufferSlice(byte[] buffer, int offset, int length, int count)
        {
            Buffer = buffer;
            Offset = offset;
            Length = length;
            Count = count;
        }

        public byte[] Buffer { get; }
        public int Offset { get; }
        public int Length { get; }
        public int Count { get; }
    }
}