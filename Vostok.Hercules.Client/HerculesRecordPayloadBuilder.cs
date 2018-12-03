using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordPayloadBuilder : IHerculesTagsBuilder
    {
        private readonly IBinaryWriter binaryWriter;

        public HerculesRecordPayloadBuilder(IBinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;
        }

        public IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Container);

            using (var builder = new HerculesRecordPayloadBuilderWithCounter(binaryWriter))
                value.Invoke(builder);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, byte value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Byte)
                        .Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, short value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Short)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, int value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Integer)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, long value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Long)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, bool value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Flag)
                        .Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, float value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Float)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, double value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Double)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, Guid value)
            => throw new NotImplementedException();

        public IHerculesTagsBuilder AddValue(string key, string value)
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

        public IHerculesTagsBuilder AddVectorOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> values)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Count > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ContainerArray)
                            .WriteCollection(
                                values,
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
                                values,
                                (writer, item) =>
                                {
                                    using (var builder = new HerculesRecordPayloadBuilderWithCounter(binaryWriter))
                                        item.Invoke(builder);
                                });
            }

            return this;
        }

        //TODO: add overload for byte array to interface?
        public IHerculesTagsBuilder AddVector(string key, byte[] values)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ByteArray)
                            .WriteWithInt32LengthPrefix(values);
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ByteVector)
                            .WriteWithByteLengthPrefix(values);
            }

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<byte> values)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Count > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ByteArray)
                            .WriteCollection(values, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ByteVector)
                            .WriteVector(values, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<short> value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Count > 255)
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

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<int> value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Count > 255)
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

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<long> value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Count > 255)
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

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<bool> value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Count > 255)
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

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<float> value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Count > 255)
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

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<double> value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Count > 255)
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

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<Guid> values) =>
            throw new NotImplementedException();

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<string> value)
        {
            binaryWriter.WriteWithByteLengthPrefix(key);

            if (value.Count > 255)
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