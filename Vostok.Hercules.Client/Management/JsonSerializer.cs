using System;
using System.Runtime.Serialization.Json;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Utilities;

namespace Vostok.Hercules.Client.Management
{
    internal class JsonSerializer
    {
        private readonly DataContractJsonSerializer streamDtoSerializer = new DataContractJsonSerializer(typeof(StreamDescriptionDto));
        private readonly DataContractJsonSerializer timelineDtoSerializer = new DataContractJsonSerializer(typeof(TimelineDescriptionDto));
        private readonly DataContractJsonSerializer stringArraySerializer = new DataContractJsonSerializer(typeof(string[]));

        public ArraySegment<byte> Serialize(CreateStreamQuery query)
            => streamDtoSerializer.SerializeObject(new StreamDescriptionDto(query));

        public StreamDescription DeserializeStreamDescription(ArraySegment<byte> content)
            => streamDtoSerializer.DeserializeObject<StreamDescriptionDto>(content).ToDescription();

        public ArraySegment<byte> Serialize(CreateTimelineQuery query)
            => timelineDtoSerializer.SerializeObject(new TimelineDescriptionDto(query));

        public TimelineDescription DeserializeTimelineDescription(ArraySegment<byte> content)
            => timelineDtoSerializer.DeserializeObject<TimelineDescriptionDto>(content).ToDescription();

        public string[] DeserializeStringArray(ArraySegment<byte> content) =>
            stringArraySerializer.DeserializeObject<string[]>(content);
    }
}