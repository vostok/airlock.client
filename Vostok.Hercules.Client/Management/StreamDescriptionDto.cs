using System.Runtime.Serialization;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Management
{
    [DataContract]
    internal class StreamDescriptionDto
    {
        [DataMember(Name = "name", IsRequired = true)]
        public string Name;

        [DataMember(Name = "type", IsRequired = true)]
        public string Type;

        [DataMember(Name = "partitions")]
        public int Partitions;

        [DataMember(Name = "ttl")]
        public long TtlMilliseconds;

        [CanBeNull, DataMember(Name = "shardingKey", EmitDefaultValue = false)]
        public string[] ShardingKey;

        [CanBeNull, DataMember(Name = "sources", EmitDefaultValue = false)]
        public string[] Sources;
    }
}