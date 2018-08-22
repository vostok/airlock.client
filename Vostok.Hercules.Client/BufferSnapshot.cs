namespace Vostok.Hercules.Client
{
    internal class BufferSnapshot
    {
        public BufferSnapshot(IBuffer parent, byte[] buffer, int position, int recordsCount)
        {
            Parent = parent;
            Buffer = buffer;
            Position = position;
            RecordsCount = recordsCount;
        }

        public IBuffer Parent { get; }
        public byte[] Buffer { get; }
        public int Position { get; }
        public int RecordsCount { get; }
    }
}