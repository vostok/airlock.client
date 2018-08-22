using System;
using System.Linq;

namespace Vostok.Hercules.Client.TimeBasedUuid
{
    internal class TimeGuid
    {
        public const int Size = 16;

        private static readonly TimeGuidGenerator GuidGenerator =
            new TimeGuidGenerator(new PreciseTimestampGenerator(TimeSpan.FromSeconds(1), TimeSpan.FromMilliseconds(100)));

        private readonly byte[] bytes;

        private TimeGuid(byte[] bytes)
        {
            if (TimeGuidBitsLayout.GetVersion(bytes) != GuidVersion.TimeBased)
                throw new ArgumentException($"Invalid timeguid: [{string.Join(", ", bytes.Select(x => x.ToString("x2")))}]");
            this.bytes = bytes;
        }

        public static TimeGuid Now() =>
            new TimeGuid(GuidGenerator.NewGuid());

        public static TimeGuid New(long timestamp) =>
            new TimeGuid(GuidGenerator.NewGuid(timestamp));

        public byte[] ToByteArray() => bytes;
    }
}