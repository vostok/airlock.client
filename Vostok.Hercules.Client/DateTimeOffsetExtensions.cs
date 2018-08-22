using System;

namespace Vostok.Hercules.Client
{
    internal static class DateTimeOffsetExtensions
    {
        private static readonly long UnixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public static long ToUnixTimeNanoseconds(this DateTimeOffset source) =>
            (source.UtcTicks - UnixEpochTicks) * 100;
    }
}