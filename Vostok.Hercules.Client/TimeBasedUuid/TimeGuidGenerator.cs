using Vostok.Commons.Helpers.Conversions;

namespace Vostok.Hercules.Client.TimeBasedUuid
{
    internal class TimeGuidGenerator : ITimeGuidGenerator
    {
        private readonly PreciseTimestampGenerator preciseTimestampGenerator = new PreciseTimestampGenerator(1.Seconds(), 100.Milliseconds());

        public TimeGuid NewGuid() =>
            new TimeGuid(TimeGuidBitsLayout.Format(preciseTimestampGenerator.NowTicks(), GenerateRandomClockSequence(), GenerateRandomNode()));

        public TimeGuid NewGuid(long timestamp) =>
            new TimeGuid(TimeGuidBitsLayout.Format(timestamp, GenerateRandomClockSequence(), GenerateRandomNode()));

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