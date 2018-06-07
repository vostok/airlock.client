using System;

namespace Vostok.Airlock.Client.Abstractions
{
    public static class DateTimeOffsetExtensions
    {
        private static readonly long unixEpochTicks = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;

        public static long ToUnixTimeNanoseconds(this DateTimeOffset source)
        {
            return (source.UtcTicks - unixEpochTicks) * 100;
        }
    }
}