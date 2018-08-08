using System;
using System.Linq;

namespace Vostok.Airlock.Client.TimeBasedUuid
{
    internal class TimeGuid
    {
        public const int Size = 16;

        private static readonly TimeGuidGenerator guidGenerator = new TimeGuidGenerator(new PreciseTimestampGenerator(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)));

        private readonly byte[] bytes;

        private TimeGuid(byte[] bytes)
        {
            if (TimeGuidBitsLayout.GetVersion(bytes) != GuidVersion.TimeBased)
                throw new ArgumentException($"Invalid timeguid: [{string.Join(", ", bytes.Select(x => x.ToString("x2")))}]");
            this.bytes = bytes;
        }

        public static TimeGuid Now()
        {
            return new TimeGuid(guidGenerator.NewGuid());
        }

        public static TimeGuid New(long timestamp)
        {
            return new TimeGuid(guidGenerator.NewGuid(timestamp));
        }

        public byte[] ToByteArray()
        {
            return bytes;
        }
    }
}