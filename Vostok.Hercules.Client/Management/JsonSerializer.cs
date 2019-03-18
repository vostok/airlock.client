using System;
using System.IO;
using System.Runtime.Serialization.Json;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;

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
            => SerializeObject(streamDtoSerializer, new StreamDescriptionDto(query));

        private StreamDescription DeserializeStreamDescription(ArraySegment<byte> content)
            => DeserializeObject<StreamDescriptionDto>(streamDtoSerializer, content).ToDescription();

        private ArraySegment<byte> Serialize(CreateTimelineQuery query)
            => SerializeObject(timelineDtoSerializer, new TimelineDescriptionDto(query));

        private TimelineDescription DeserializeTimelineDescription(ArraySegment<byte> content)
            => DeserializeObject<TimelineDescriptionDto>(timelineDtoSerializer, content).ToDescription();

        public string[] DeserializeStringArray(ArraySegment<byte> content) =>
            DeserializeObject<string[]>(stringArraySerializer, content);

        private static ArraySegment<byte> SerializeObject(DataContractJsonSerializer serializer, object item)
        {
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, item);
                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
            }
        }

        private static T DeserializeObject<T>(DataContractJsonSerializer serializer, ArraySegment<byte> data)
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            using (var memoryStream = new MemoryStream(data.Array, data.Offset, data.Count))
            {
                return (T) serializer.ReadObject(memoryStream);
            }
        }
    }
}