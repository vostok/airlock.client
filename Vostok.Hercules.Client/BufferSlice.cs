namespace Vostok.Hercules.Client
{
    internal class BufferSlice
    {
        public BufferSlice(IBuffer parent, byte[] buffer, int offset, int length, int recordsCount)
        {
            Parent = parent;
            Buffer = buffer;
            Offset = offset;
            Length = length;
            RecordsCount = recordsCount;
        }

        public IBuffer Parent { get; }
        public byte[] Buffer { get; }
        public int Offset { get; }
        public int Length { get; }
        public int RecordsCount { get; }
    }
}