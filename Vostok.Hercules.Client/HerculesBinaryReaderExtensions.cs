using System;
using System.Linq;
using System.Text;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.TimeBasedUuid;

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
            var timeGuid = new TimeGuid(reader.ReadByteArray(16));
            builder.SetTimestamp( /* TODO: parse timestamp from timeguid or drop timeguid */ default);
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
                    case TagValueTypeDefinition.Text:
                        builder.AddValue(key, reader.ReadString());
                        break;
                    case TagValueTypeDefinition.ContainerArray:
                        length = reader.ReadInt32();
                        builder.AddVectorOfContainers(key, Enumerable.Repeat(readContainer, length).ToArray());
                        break;
                    case TagValueTypeDefinition.ByteArray:
                        builder.AddVector(key, reader.ReadByteArray());
                        break;
                    case TagValueTypeDefinition.ShortArray:
                        builder.AddVector(key, reader.ReadArray(r => r.ReadInt16()));
                        break;
                    case TagValueTypeDefinition.IntegerArray:
                        builder.AddVector(key, reader.ReadArray(r => r.ReadInt32()));
                        break;
                    case TagValueTypeDefinition.LongArray:
                        builder.AddVector(key, reader.ReadArray(r => r.ReadInt64()));
                        break;
                    case TagValueTypeDefinition.FlagArray:
                        builder.AddVector(key, reader.ReadArray(r => r.ReadBool()));
                        break;
                    case TagValueTypeDefinition.FloatArray:
                        builder.AddVector(key, reader.ReadArray(r => r.ReadFloat()));
                        break;
                    case TagValueTypeDefinition.DoubleArray:
                        builder.AddVector(key, reader.ReadArray(r => r.ReadDouble()));
                        break;
                    case TagValueTypeDefinition.StringArray:
                        builder.AddVector(key, reader.ReadArray(r => r.ReadShortString()));
                        break;
                    case TagValueTypeDefinition.TextArray:
                        builder.AddVector(key, reader.ReadArray(r => r.ReadString()));
                        break;
                    case TagValueTypeDefinition.ContainerVector:
                        length = reader.ReadByte();
                        builder.AddVectorOfContainers(key, Enumerable.Repeat(readContainer, length).ToArray());
                        break;
                    case TagValueTypeDefinition.ByteVector:
                        builder.AddVector(key, reader.ReadVector( r => r.ReadByte()));
                        break;
                    case TagValueTypeDefinition.ShortVector:
                        builder.AddVector(key, reader.ReadVector( r => r.ReadInt16()));
                        break;
                    case TagValueTypeDefinition.IntegerVector:
                        builder.AddVector(key, reader.ReadVector( r => r.ReadInt32()));
                        break;
                    case TagValueTypeDefinition.LongVector:
                        builder.AddVector(key, reader.ReadVector( r => r.ReadInt64()));
                        break;
                    case TagValueTypeDefinition.FlagVector:
                        builder.AddVector(key, reader.ReadVector( r => r.ReadBool()));
                        break;
                    case TagValueTypeDefinition.FloatVector:
                        builder.AddVector(key, reader.ReadVector( r => r.ReadFloat()));
                        break;
                    case TagValueTypeDefinition.DoubleVector:
                        builder.AddVector(key, reader.ReadVector( r => r.ReadDouble()));
                        break;
                    case TagValueTypeDefinition.StringVector:
                        builder.AddVector(key, reader.ReadVector( r => r.ReadShortString()));
                        break;
                    case TagValueTypeDefinition.TextVector:
                        builder.AddVector(key, reader.ReadVector( r => r.ReadByte()));
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