using Vostok.Commons.Threading;
using Vostok.Commons.Time;

namespace Vostok.Hercules.Client.TimeBasedUuid
{
    internal class TimeGuidGenerator : ITimeGuidGenerator
    {
        public TimeGuid NewGuid() =>
            new TimeGuid(TimeGuidBitsLayout.Format(PreciseDateTime.UtcNow.UtcTicks, GenerateRandomClockSequence(), GenerateRandomNode()));

        public TimeGuid NewGuid(long timestamp) =>
            new TimeGuid(TimeGuidBitsLayout.Format(timestamp, GenerateRandomClockSequence(), GenerateRandomNode()));

        // should it be generated only once?
        // https://docs.google.com/document/d/1VJq95AoBrxSfFR2KX21xGi1INPP9CPBtwEmnLlHnkhA/edit#heading=h.4ll19u6asb8k
        private static byte[] GenerateRandomNode()
        {
            var buffer = new byte[TimeGuidBitsLayout.NodeSize];
            ThreadSafeRandom.NextBytes(buffer);
            return buffer;
        }

        private static ushort GenerateRandomClockSequence() =>
            (ushort) ThreadSafeRandom.Next(TimeGuidBitsLayout.MinClockSequence, TimeGuidBitsLayout.MaxClockSequence + 1);
    }
}