using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;

namespace Vostok.Hercules.Client.Management
{
    internal static class TimelineDescriptionDtoConverter
    {
        [NotNull]
        public static TimelineDescriptionDto CreateFromQuery([NotNull] CreateTimelineQuery query)
            => new TimelineDescriptionDto
            {
                Name = query.Name,
                Streams = query.Sources,
                Slices = query.Slices ?? ManagementClientDefaults.TimelineSlices,
                TtlMilliseconds = (long)(query.TTL ?? ManagementClientDefaults.TimelineTTL).TotalMilliseconds,
                TimetrapSizeMilliseconds = (long)(query.TimetrapSize ?? ManagementClientDefaults.TimetrapSize).TotalMilliseconds,
                ShardingKey = query.ShardingKey
            };

        [CanBeNull]
        public static TimelineDescription ConvertToDescription([CanBeNull] TimelineDescriptionDto dto)
            => dto == null ? null : new TimelineDescription(dto.Name)
            {
                Sources = dto.Streams,
                Slices = dto.Slices,
                TTL = TimeSpan.FromMilliseconds(dto.TtlMilliseconds),
                TimetrapSize = TimeSpan.FromMilliseconds(dto.TimetrapSizeMilliseconds),
                ShardingKey = dto.ShardingKey
            };
    }
}