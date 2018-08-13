using System;
using System.Linq;
using Vostok.Airlock.Client.Abstractions;
using Vostok.Airlock.Client.Binary;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordBuilder : IAirlockRecordBuilder
    {
        private readonly IBinaryWriter binaryWriter;

        public AirlockRecordBuilder(IBinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;
        }

        internal long Timestamp { get; private set; }
        internal short TagsCount { get; private set; }

        public IAirlockRecordBuilder SetTimestamp(DateTimeOffset timestamp)
        {
            Timestamp = timestamp.ToUniversalTime().ToUnixTimeNanoseconds();
            return this;
        }

        public IAirlockRecordBuilder Add(string key, byte value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Byte)
                        .Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, short value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Short)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, int value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Integer)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, long value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Long)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, bool value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Flag)
                        .Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, float value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Float)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, double value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Double)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, string value)
        {
            TagsCount++;

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

        public IAirlockRecordBuilder Add(string key, byte[] values)
        {
            TagsCount++;

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

        public IAirlockRecordBuilder Add(string key, short[] values)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ShortArray)
                            .WriteCollection(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ShortVector)
                            .WriteVector(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, int[] values)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.IntegerArray)
                            .WriteCollection(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.IntegerVector)
                            .WriteVector(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, long[] values)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.LongArray)
                            .WriteCollection(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.LongVector)
                            .WriteVector(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, bool[] values)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.FlagArray)
                            .WriteCollection(values, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.FlagVector)
                            .WriteVector(values, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, float[] values)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.FloatArray)
                            .WriteCollection(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.FloatVector)
                            .WriteVector(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, double[] values)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.DoubleArray)
                            .WriteCollection(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.DoubleVector)
                            .WriteVector(values, (writer, item) => writer.WriteInNetworkByteOrder(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, string[] values)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key);

            if (values.Length > 255)
            {
                if (values.Any(x => x.Length > 255))
                {
                    binaryWriter.Write((byte)TagValueTypeDefinition.TextArray)
                                .WriteCollection(values, (writer, item) => writer.WriteWithInt32LengthPrefix(item));
                }
                else
                {
                    binaryWriter.Write((byte)TagValueTypeDefinition.StringArray)
                                .WriteCollection(values, (writer, item) => writer.WriteWithByteLengthPrefix(item));
                }
            }
            else
            {
                if (values.Any(x => x.Length > 255))
                {
                    binaryWriter.Write((byte)TagValueTypeDefinition.TextVector)
                                .WriteVector(values, (writer, item) => writer.WriteWithInt32LengthPrefix(item));
                }
                else
                {
                    binaryWriter.Write((byte)TagValueTypeDefinition.StringVector)
                                .WriteVector(values, (writer, item) => writer.WriteWithByteLengthPrefix(item));
                }
            }

            return this;
        }
    }
}