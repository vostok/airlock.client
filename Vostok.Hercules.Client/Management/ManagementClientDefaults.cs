using System;

namespace Vostok.Hercules.Client.Management
{
    internal static class ManagementClientDefaults
    {
        public const int StreamPartitions = 3;

        public const int TimelineSlices = 3;

        public static readonly TimeSpan StreamTTL = TimeSpan.FromDays(3);

        public static readonly TimeSpan TimelineTTL = TimeSpan.FromDays(3);

        public static readonly TimeSpan TimetrapSize = TimeSpan.FromSeconds(1);
    }
}