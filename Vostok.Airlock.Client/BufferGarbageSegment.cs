namespace Vostok.Airlock.Client
{
    internal class BufferGarbageSegment : ILineSegment
    {
        public int Offset { get; set; }
        public int Length { get; set; }
        public int RecordsCount { get; set; }
    }
}