using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;

namespace Vostok.Hercules.Client.Management
{
    internal static class StreamDescriptionDtoConverter
    {
        [NotNull]
        public static StreamDescriptionDto CreateFromQuery([NotNull] CreateStreamQuery query)
            => new StreamDescriptionDto
            {
                Name = query.Name,
                Type = query.Type == StreamType.Base ? "base" : "derived",
                Partitions = query.Partitions ?? ManagementClientDefaults.StreamPartitions,
                TtlMilliseconds = (long)(query.TTL ?? ManagementClientDefaults.StreamTTL).TotalMilliseconds,
                ShardingKey = query.ShardingKey,
                Sources = query.Sources
            };

        [CanBeNull]
        public static StreamDescription ConvertToDescription([CanBeNull] StreamDescriptionDto dto)
            => dto == null ? null : new StreamDescription(dto.Name)
            {
                Type = (StreamType) Enum.Parse(typeof(StreamType), dto.Type, true),
                Partitions = dto.Partitions,
                TTL = TimeSpan.FromMilliseconds(dto.TtlMilliseconds),
                ShardingKey = dto.ShardingKey,
                Sources = dto.Sources,
            };
    }
}
