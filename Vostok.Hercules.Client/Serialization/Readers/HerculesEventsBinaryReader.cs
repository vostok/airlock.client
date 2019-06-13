using System;
using System.Collections.Generic;
using Vostok.Commons.Binary;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client.Serialization.Readers
{
    internal class HerculesEventsBinaryReader : IHerculesEventsBinaryReader<HerculesEvent>
    {
        public IList<HerculesEvent> Read(byte[] bytes, int offset)
        {
            var reader = new BinaryBufferReader(bytes, offset)
            {
                Endianness = Endianness.Big
            };

            return reader.ReadArray(ReadEvent);
        }

        public static HerculesEvent ReadEvent(IBinaryReader reader)
        {
            reader.EnsureBigEndian();

            var builder = new HerculesEventBuilder();

            var version = reader.ReadByte();
            if (version != Constants.EventProtocolVersion)
                throw new NotSupportedException($"Unsupported Hercules protocol version: {version}");

            var utcTimestamp = EpochHelper.FromUnixTimeUtcTicks(reader.ReadInt64());

            builder.SetTimestamp(new DateTimeOffset(utcTimestamp, TimeSpan.Zero));

            reader.Position += 16;
            ReadContainer(reader, builder);

            return builder.BuildEvent();
        }

        private static void ReadContainer(IBinaryReader reader, IHerculesTagsBuilder builder)
        {
            var tagsCount = reader.ReadInt16();

            for (var i = 0; i < tagsCount; i++)
            {
                var key = reader.ReadShortString();
                var valueType = (TagType)reader.ReadByte();

                switch (valueType)
                {
                    case TagType.Container:
                        builder.AddContainer(key, b => ReadContainer(reader, b));
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
            var elementType = (TagType)reader.ReadByte();

            switch (elementType)
            {
                case TagType.Container:
                    var elementsCount = reader.ReadInt32();
                    var valueBuilder = new Action<IHerculesTagsBuilder>(b => ReadContainer(reader, b));
                    var valueBuilders = new Action<IHerculesTagsBuilder>[elementsCount];

                    for (var i = 0; i < elementsCount; i++)
                        valueBuilders[i] = valueBuilder;

                    builder.AddVectorOfContainers(key, valueBuilders);
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
                    throw new NotSupportedException("Vectors of nulls are not supported yet.");

                case TagType.Vector:
                    throw new NotSupportedException("Nested vectors are not supported yet.");

                default:
                    throw new ArgumentOutOfRangeException(nameof(elementType), elementType, "Unexpected vector element type.");
            }
        }
    }
}