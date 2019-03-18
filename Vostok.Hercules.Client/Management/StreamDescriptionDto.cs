using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;

namespace Vostok.Hercules.Client.Management
{
    [DataContract]
    internal class StreamDescriptionDto
    {
        private const int DefaultPartitionsCount = 3;
        private static readonly TimeSpan DefaultTTL = TimeSpan.FromDays(3);
        
        public StreamDescriptionDto(CreateStreamQuery query)
        {
            name = query.Name;
            type = query.Type.ToString().ToLowerInvariant();
            partitions = query.Partitions ?? DefaultPartitionsCount;
            ttl = (long) (query.TTL ?? DefaultTTL).TotalMilliseconds;
            sources = query.Sources;
            shardingKey = query.ShardingKey ?? Array.Empty<string>();
        }

        [DataMember]
        public string name;

        [DataMember]
        public string type;

        [DataMember]
        public int partitions;

        [DataMember]
        public long ttl;

        [CanBeNull, DataMember(EmitDefaultValue = false)]
        public string[] shardingKey;

        [CanBeNull, DataMember(EmitDefaultValue = false)]
        public string[] sources;

        public StreamDescription ToDescription() =>
            new StreamDescription(name)
            {
                Partitions = partitions,
                TTL = TimeSpan.FromMilliseconds(ttl),
                ShardingKey = shardingKey,
                Type = (StreamType)Enum.Parse(typeof(StreamType), type, true),
                Sources = sources
            };
    }
}