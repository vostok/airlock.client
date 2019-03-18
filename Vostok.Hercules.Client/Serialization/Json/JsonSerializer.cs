using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Json;
using Vostok.Commons.Collections;

namespace Vostok.Hercules.Client.Serialization.Json
{
    internal class JsonSerializer
    {
        private readonly ConcurrentDictionary<Type, UnboundedObjectPool<DataContractJsonSerializer>> serializerPools
            = new ConcurrentDictionary<Type, UnboundedObjectPool<DataContractJsonSerializer>>();

        public byte[] Serialize<T>(T item)
        {
            using (ObtainSerializer<T>(out var serializer))
            {
                var buffer = new MemoryStream();

                serializer.WriteObject(buffer, item);

                return buffer.ToArray();
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            using (ObtainSerializer<T>(out var serializer))
                return (T) serializer.ReadObject(stream);
        }

        private IDisposable ObtainSerializer<T>(out DataContractJsonSerializer serializer)
        {
            return serializerPools
                .GetOrAdd(
                    typeof(T),
                    type => new UnboundedObjectPool<DataContractJsonSerializer>(() => new DataContractJsonSerializer(type)))
                .Acquire(out serializer);
        }
    }
}
