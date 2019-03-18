using System;
using System.IO;
using System.Runtime.Serialization.Json;

namespace Vostok.Hercules.Client.Utilities
{
    internal static class DataContractJsonSerializerExtensions
    {
        public static ArraySegment<byte> SerializeObject(this DataContractJsonSerializer serializer, object o)
        {
            using (var memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, o);
                return new ArraySegment<byte>(memoryStream.GetBuffer(), 0, (int)memoryStream.Position);
            }
        }

        public static T DeserializeObject<T>(this DataContractJsonSerializer serializer, ArraySegment<byte> data)
        {
            using (var memoryStream = new MemoryStream(data.Array, data.Offset, data.Count))
            {
                return (T) serializer.ReadObject(memoryStream);
            }
        }
    }
}