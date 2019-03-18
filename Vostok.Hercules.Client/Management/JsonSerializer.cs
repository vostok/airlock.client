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

        public ArraySegment<byte> Serialize(object value)
        {
            if (value is CreateStreamQuery streamQuery)
                return Serialize(streamQuery);
            if (value is CreateTimelineQuery timelineQuery)
                return Serialize(timelineQuery);
            throw new ArgumentOutOfRangeException(nameof(value));
        }
        
        public T Deserialize<T>(ArraySegment<byte> bytes)
        {
            if (typeof(T) == typeof(StreamDescription))
                return (T) (object) DeserializeStreamDescription(bytes);
            if (typeof(T) == typeof(TimelineDescription))
                return (T)(object)DeserializeTimelineDescription(bytes);
            throw new ArgumentOutOfRangeException(nameof(T), typeof(T).Name, "Unknown type.");
        }
        
        private ArraySegment<byte> Serialize(CreateStreamQuery query)
            => streamDtoSerializer.SerializeObject(new StreamDescriptionDto(query));

        private StreamDescription DeserializeStreamDescription(ArraySegment<byte> content)
            => streamDtoSerializer.DeserializeObject<StreamDescriptionDto>(content).ToDescription();

        private ArraySegment<byte> Serialize(CreateTimelineQuery query)
            => timelineDtoSerializer.SerializeObject(new TimelineDescriptionDto(query));

        private TimelineDescription DeserializeTimelineDescription(ArraySegment<byte> content)
            => timelineDtoSerializer.DeserializeObject<TimelineDescriptionDto>(content).ToDescription();

        public string[] DeserializeStringArray(ArraySegment<byte> content) =>
            stringArraySerializer.DeserializeObject<string[]>(content);
    }
}