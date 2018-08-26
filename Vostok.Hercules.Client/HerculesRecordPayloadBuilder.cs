using System;
using System.Linq;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordPayloadBuilder : IHerculesRecordPayloadBuilder
    {
        private readonly IBinaryWriter binaryWriter;

        public HerculesRecordPayloadBuilder(IBinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;
        }

        public IHerculesRecordPayloadBuilder Add(string key, Func<IHerculesRecordPayloadBuilder, IHerculesRecordPayloadBuilder> value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Container);

            using (var builder = new HerculesRecordPayloadBuilderWithCounter(binaryWriter))
                value.Invoke(builder);

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, byte value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Byte)
                        .Write(value);

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, short value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Short)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, int value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Integer)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, long value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Long)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, bool value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Flag)
                        .Write(value);

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, float value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Float)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, double value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Double)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, string value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.Text)
                            .WriteWithInt32LengthPrefix(value);
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.String)
                            .WriteWithByteLengthPrefix(value);
            }

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, Func<IHerculesRecordPayloadBuilder, IHerculesRecordPayloadBuilder>[] value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ContainerArray)
                            .WriteCollection(
                                value,
                                (writer, item) =>
                                {
                                    using (var builder = new HerculesRecordPayloadBuilderWithCounter(binaryWriter))
                                        item.Invoke(builder);
                                });
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ContainerVector)
                            .WriteVector(
                                value,
                                (writer, item) =>
                                {
                                    using (var builder = new HerculesRecordPayloadBuilderWithCounter(binaryWriter))
                                        item.Invoke(builder);
                                });
            }

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, byte[] value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ByteArray)
                            .WriteWithInt32LengthPrefix(value);
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ByteVector)
                            .WriteWithByteLengthPrefix(value);
            }

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, short[] value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ShortArray)
                            .WriteCollection(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ShortVector)
                            .WriteVector(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, int[] value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.IntegerArray)
                            .WriteCollection(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.IntegerVector)
                            .WriteVector(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, long[] value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.LongArray)
                            .WriteCollection(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.LongVector)
                            .WriteVector(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, bool[] value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.FlagArray)
                            .WriteCollection(value, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.FlagVector)
                            .WriteVector(value, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, float[] value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.FloatArray)
                            .WriteCollection(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.FloatVector)
                            .WriteVector(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, double[] value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.DoubleArray)
                            .WriteCollection(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.DoubleVector)
                            .WriteVector(value, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, string[] value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Length > 255)
            {
                if (value.Any(x => x.Length > 255))
                {
                    binaryWriter.Write((byte)TagValueTypeDefinition.TextArray)
                                .WriteCollection(value, (writer, item) => writer.WriteWithInt32LengthPrefix(item));
                }
                else
                {
                    binaryWriter.Write((byte)TagValueTypeDefinition.StringArray)
                                .WriteCollection(value, (writer, item) => writer.WriteWithByteLengthPrefix(item));
                }
            }
            else
            {
                if (value.Any(x => x.Length > 255))
                {
                    binaryWriter.Write((byte)TagValueTypeDefinition.TextVector)
                                .WriteVector(value, (writer, item) => writer.WriteWithInt32LengthPrefix(item));
                }
                else
                {
                    binaryWriter.Write((byte)TagValueTypeDefinition.StringVector)
                                .WriteVector(value, (writer, item) => writer.WriteWithByteLengthPrefix(item));
                }
            }

            return this;
        }
    }
}