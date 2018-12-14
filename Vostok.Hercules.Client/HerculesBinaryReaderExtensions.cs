using System;
using System.Linq;
using System.Text;
using Vostok.Commons.Binary;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client
{
    internal static class HerculesBinaryReaderExtensions
    {
        public static HerculesEvent ReadEvent(this IBinaryReader reader)
        {
            var builder = new Vostok.Hercules.Client.Abstractions.Events.HerculesEventBuilder();
            var version = reader.ReadByte();
            if (version != 1)
                throw new NotSupportedException();
            var timestamp = EpochHelper.FromUnixTimeUtcTicks(reader.ReadInt64());
            builder.SetTimestamp(timestamp);
            reader.ReadContainer(builder);
            return builder.BuildEvent();
        }

        private static void ReadContainer(this IBinaryReader reader, IHerculesTagsBuilder builder)
        {
            var count = reader.ReadInt16();

            for (var i = 0; i < count; i++)
            {
                var key = ReadShortString(reader);
                var valueType = (TagValueTypeDefinition) reader.ReadByte();

                int length;

                Action<IHerculesTagsBuilder> readContainer = tagsBuilder => reader.ReadContainer(tagsBuilder);
                
                switch (valueType)
                {
                    case TagValueTypeDefinition.Container:
                        builder.AddContainer(key, readContainer);
                        break;
                    case TagValueTypeDefinition.Byte:
                        builder.AddValue(key, reader.ReadByte());
                        break;
                    case TagValueTypeDefinition.Short:
                        builder.AddValue(key, reader.ReadInt16());
                        break;
                    case TagValueTypeDefinition.Integer:
                        builder.AddValue(key, reader.ReadInt32());
                        break;
                    case TagValueTypeDefinition.Long:
                        builder.AddValue(key, reader.ReadInt64());
                        break;
                    case TagValueTypeDefinition.Flag:
                        builder.AddValue(key, reader.ReadBool());
                        break;
                    case TagValueTypeDefinition.Float:
                        builder.AddValue(key, reader.ReadFloat());
                        break;
                    case TagValueTypeDefinition.Double:
                        builder.AddValue(key, reader.ReadDouble());
                        break;
                    case TagValueTypeDefinition.String:
                        builder.AddValue(key, reader.ReadShortString());
                        break;
                    case TagValueTypeDefinition.UUID:
                        builder.AddValue(key, reader.ReadGuid());
                        break;
                    case TagValueTypeDefinition.Null:
                        break;
                    case TagValueTypeDefinition.Vector:
                        //TODO
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private static string ReadShortString(this IBinaryReader reader)
        {
            var length = reader.ReadByte();
            //TODO: avoid array allocation
            return Encoding.UTF8.GetString(reader.ReadByteArray(length));
        }

        private static T[] ReadVector<T>(this IBinaryReader reader, Func<IBinaryReader, T> readSingleValue)
        {
            var arr = new T[reader.ReadByte()];
            for (var i = 0; i < arr.Length; i++)
                arr[i] = readSingleValue(reader);
            return arr;
        }
    }
}