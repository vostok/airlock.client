namespace Vostok.Hercules.Client
{
    internal class BufferSlice
    {
        public BufferSlice(IBuffer parrent, byte[] buffer, int offset, int length, int recordsCount)
        {
            Parrent = parrent;
            Buffer = buffer;
            Offset = offset;
            Length = length;
            RecordsCount = recordsCount;
        }

        public IBuffer Parrent { get; }
        public byte[] Buffer { get; }
        public int Offset { get; }
        public int Length { get; }
        public int RecordsCount { get; }
    }
}