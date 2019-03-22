using System;
using System.Collections.Concurrent;
using System.IO;
using System.Runtime.Serialization.Json;
using JetBrains.Annotations;
using Vostok.Commons.Collections;

namespace Vostok.Hercules.Client.Serialization.Json
{
    internal class JsonSerializer
    {
        private readonly ConcurrentDictionary<Type, UnboundedObjectPool<DataContractJsonSerializer>> serializerPools
            = new ConcurrentDictionary<Type, UnboundedObjectPool<DataContractJsonSerializer>>();

        public byte[] Serialize([NotNull] object item)
        {
            using (ObtainSerializer(item.GetType(), out var serializer))
            {
                var buffer = new MemoryStream();

                serializer.WriteObject(buffer, item);

                return buffer.ToArray();
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            using (ObtainSerializer<T>(out var serializer))
                return (T)serializer.ReadObject(stream);
        }

        private IDisposable ObtainSerializer<T>(out DataContractJsonSerializer serializer)
            => ObtainSerializer(typeof(T), out serializer);

        private IDisposable ObtainSerializer(Type type, out DataContractJsonSerializer serializer)
            => serializerPools
                .GetOrAdd(type, t => new UnboundedObjectPool<DataContractJsonSerializer>(() => new DataContractJsonSerializer(t)))
                .Acquire(out serializer);
    }
}