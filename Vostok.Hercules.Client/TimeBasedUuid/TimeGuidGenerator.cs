namespace Vostok.Hercules.Client.TimeBasedUuid
{
    internal class TimeGuidGenerator
    {
        private readonly PreciseTimestampGenerator preciseTimestampGenerator;

        public TimeGuidGenerator(PreciseTimestampGenerator preciseTimestampGenerator)
        {
            this.preciseTimestampGenerator = preciseTimestampGenerator;
        }

        public byte[] NewGuid()
        {
            var nowTimestamp = preciseTimestampGenerator.NowTicks();
            return TimeGuidBitsLayout.Format(nowTimestamp, GenerateRandomClockSequence(), GenerateRandomNode());
        }

        public byte[] NewGuid(long timestamp) =>
            TimeGuidBitsLayout.Format(timestamp, GenerateRandomClockSequence(), GenerateRandomNode());

        private static byte[] GenerateRandomNode()
        {
            var buffer = new byte[TimeGuidBitsLayout.NodeSize];
            ThreadLocalRandom.Instance.NextBytes(buffer);
            return buffer;
        }

        private static ushort GenerateRandomClockSequence() =>
            (ushort) ThreadLocalRandom.Instance.Next(TimeGuidBitsLayout.MinClockSequence, TimeGuidBitsLayout.MaxClockSequence + 1);
    }
}