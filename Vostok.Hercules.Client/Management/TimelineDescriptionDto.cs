using System;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;

namespace Vostok.Hercules.Client.Management
{
    [DataContract]
    internal class TimelineDescriptionDto
    {
        //TODO: choose better default.
        private const int DefaultSlicesCount = 3;
        private static readonly TimeSpan DefaultTTL = TimeSpan.FromDays(3);
        private static readonly TimeSpan DefaultTimetrapSize = TimeSpan.FromSeconds(30);
        
        public TimelineDescriptionDto(CreateTimelineQuery query)
        {
            name = query.Name;
            sources = query.Sources;
            slices = query.Slices ?? DefaultSlicesCount;
            ttl = (long) (query.TTL ?? DefaultTTL).TotalMilliseconds;
            timetrapSize = (long) (query.TimetrapSize ?? DefaultTimetrapSize).TotalMilliseconds;
            shardingKey = query.ShardingKey;
        }

        [NotNull, DataMember]
        public string name;

        [NotNull, ItemNotNull, DataMember]
        public string[] sources;

        [DataMember]
        public int slices;

        [DataMember]
        public long ttl;

        [DataMember]
        public long timetrapSize;

        [CanBeNull, ItemNotNull, DataMember(EmitDefaultValue = false)]
        public string[] shardingKey;

        public TimelineDescription ToDescription() =>
            new TimelineDescription(name)
            {
                Sources = sources,
                Name = name,
                ShardingKey = shardingKey,
                Slices = slices,
                TimetrapSize = TimeSpan.FromMilliseconds(timetrapSize),
                TTL = TimeSpan.FromMilliseconds(ttl)
            };
    }
}