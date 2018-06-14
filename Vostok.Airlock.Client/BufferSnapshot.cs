namespace Vostok.Airlock.Client
{
    internal class BufferSnapshot
    {
        public BufferSnapshot(byte[] buffer, int bufferPosition, int recordsCount)
        {
            Buffer = buffer;
            BufferPosition = bufferPosition;
            RecordsCount = recordsCount;
        }

        public byte[] Buffer { get; }
        public int BufferPosition { get; }
        public int RecordsCount { get; }
    }
}