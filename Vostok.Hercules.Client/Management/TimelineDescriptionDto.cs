using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Management
{
    [DataContract]
    internal class TimelineDescriptionDto
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name;

        [DataMember(Name = "slices")]
        public int Slices;

        [DataMember(Name = "timetrapSize")]
        public long TimetrapSizeMilliseconds;

        [DataMember(Name = "ttl")]
        public long TtlMilliseconds;

        [CanBeNull]
        [DataMember(Name = "shardingKey", EmitDefaultValue = false)]
        public string[] ShardingKey;

        [CanBeNull]
        [DataMember(Name = "streams", EmitDefaultValue = false)]
        public string[] Streams;
    }
}