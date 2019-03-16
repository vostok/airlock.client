using System;
using System.Linq;
using System.Text;
using Vostok.Commons.Binary;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Serialization.Builders;

namespace Vostok.Hercules.Client.Serialization.Readers
{
    internal static class BinaryEventReader
    {
        public static HerculesEvent ReadEvent(IBinaryReader reader)
        {
            var builder = new HerculesEventBuilder();
            var version = reader.ReadByte();
            if (version != Constants.ProtocolVersion)
                throw new NotSupportedException($"Unsupported Hercules protocol version: {version}");

            builder.SetTimestamp(EpochHelper.FromUnixTimeUtcTicks(reader.ReadInt64()));

            reader.Position += 16;
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

                switch (valueType)
                {
                    case TagType.Container:
                        builder.AddContainer(key, reader.ReadContainer);
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
                    case TagType.Uuid:
                        builder.AddValue(key, reader.ReadGuid());
                        break;
                    case TagType.Null:
                        builder.AddNull(key);
                        break;
                    case TagType.Vector:
                        ReadVector(reader, builder, key);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(valueType), valueType, "Unexpected tag value type.");
                }
            }
        }

        private static void ReadVector(IBinaryReader reader, IHerculesTagsBuilder builder, string key)
        {
            var elementType = (TagType) reader.ReadByte();

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
                    builder.AddVector(key, reader.ReadByteArray());
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
                case TagType.Uuid:
                    builder.AddVector(key, reader.ReadArray(r => r.ReadGuid()));
                    break;
                case TagType.Null:
                    builder.AddNull(key);
                    break;
                case TagType.Vector:
                    throw new NotSupportedException("Nested vectors are not supported yet.");
                default:
                    throw new ArgumentOutOfRangeException(nameof(elementType), elementType, "Unexpected vector element type.");
            }
        }

        private static string ReadShortString(this IBinaryReader reader)
        {
            var length = reader.ReadByte();

            if (reader is BinaryBufferReader bufferReader)
            {
                var result = Encoding.UTF8.GetString(bufferReader.Buffer, (int) bufferReader.Position, length);

                bufferReader.Position += length;

                return result;
            }

            return Encoding.UTF8.GetString(reader.ReadByteArray(length));
        }
    }
}
