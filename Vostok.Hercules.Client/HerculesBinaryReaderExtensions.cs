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
            var builder = new Abstractions.Events.HerculesEventBuilder();
            var version = reader.ReadByte();
            if (version != 1)
                throw new NotSupportedException();
            var timestamp = EpochHelper.FromUnixTimeUtcTicks(reader.ReadInt64());
            builder.SetTimestamp(timestamp);
            reader.ReadGuid();
            reader.ReadContainer(builder);
            return builder.BuildEvent();
        }

        private static void ReadContainer(this IBinaryReader reader, IHerculesTagsBuilder builder)
        {
            var count = reader.ReadInt16();

            for (var i = 0; i < count; i++)
            {
                var key = ReadShortString(reader);
                var valueType = (TagType)reader.ReadByte();

                Action<IHerculesTagsBuilder> readContainer = tagsBuilder => reader.ReadContainer(tagsBuilder);

                switch (valueType)
                {
                    case TagType.Container:
                        builder.AddContainer(key, readContainer);
                        break;
                    case TagType.Byte:
                        builder.AddValue(key, reader.ReadByte());
                        break;
                    case TagType.Short:
                        builder.AddValue(key, reader.ReadInt16());
                        break;
                    case TagType.Integer:
                        builder.AddValue(key, reader.ReadInt32());
                        break;
                    case TagType.Long:
                        builder.AddValue(key, reader.ReadInt64());
                        break;
                    case TagType.Flag:
                        builder.AddValue(key, reader.ReadBool());
                        break;
                    case TagType.Float:
                        builder.AddValue(key, reader.ReadFloat());
                        break;
                    case TagType.Double:
                        builder.AddValue(key, reader.ReadDouble());
                        break;
                    case TagType.String:
                        builder.AddValue(key, reader.ReadString());
                        break;
                    case TagType.UUID:
                        builder.AddValue(key, reader.ReadGuid());
                        break;
                    case TagType.Null:
                        builder.AddNull(key);
                        break;
                    case TagType.Vector:
                        ReadVector(reader, builder, key);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(valueType),
                            valueType,
                            "Value type is not defined");
                }
            }
        }

        private static void ReadVector(IBinaryReader reader, IHerculesTagsBuilder builder, string key)
        {
            var elementType = (TagType)reader.ReadByte();

            switch (elementType)
            {
                case TagType.Container:
                    builder.AddVectorOfContainers(
                        key,
                        Enumerable
                            .Range(0, reader.ReadInt32())
                            .Select(x => new Action<IHerculesTagsBuilder>(b => ReadContainer(reader, b)))
                            .ToList());
                    break;
                case TagType.Byte:
                    var length = reader.ReadInt32();
                    builder.AddVector(key, reader.ReadByteArray(length));
                    break;
                case TagType.Short:
                    builder.AddVector(key, reader.ReadArray(r => r.ReadInt16()));
                    break;
                case TagType.Integer:
                    builder.AddVector(key, reader.ReadArray(r => r.ReadInt32()));
                    break;
                case TagType.Long:
                    builder.AddVector(key, reader.ReadArray(r => r.ReadInt64()));
                    break;
                case TagType.Flag:
                    builder.AddVector(key, reader.ReadArray(r => r.ReadBool()));
                    break;
                case TagType.Float:
                    builder.AddVector(key, reader.ReadArray(r => r.ReadFloat()));
                    break;
                case TagType.Double:
                    builder.AddVector(key, reader.ReadArray(r => r.ReadDouble()));
                    break;
                case TagType.String:
                    builder.AddVector(key, reader.ReadArray(r => r.ReadString()));
                    break;
                case TagType.UUID:
                    builder.AddVector(key, reader.ReadArray(r => r.ReadGuid()));
                    break;
                case TagType.Null:
                case TagType.Vector:
                    throw new NotSupportedException();
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(elementType),
                        elementType,
                        "Element value type is not defined.");
            }
        }

        private static string ReadShortString(this IBinaryReader reader)
        {
            var length = reader.ReadByte();
            return Encoding.UTF8.GetString(reader.ReadByteArray(length));
        }
    }
}