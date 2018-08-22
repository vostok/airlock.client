using System;
using System.Linq;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordBuilder : IHerculesRecordBuilder
    {
        private readonly IBinaryWriter binaryWriter;

        public HerculesRecordBuilder(IBinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;
        }

        internal long Timestamp { get; private set; }
        internal short TagsCount { get; private set; }

        public IHerculesRecordBuilder SetTimestamp(DateTimeOffset timestamp)
        {
            Timestamp = timestamp.ToUniversalTime().ToUnixTimeNanoseconds();
            return this;
        }

        public IHerculesRecordBuilder Add(string key, byte value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Byte)
                        .Write(value);

            return this;
        }

        public IHerculesRecordBuilder Add(string key, short value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Short)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordBuilder Add(string key, int value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Integer)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordBuilder Add(string key, long value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Long)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordBuilder Add(string key, bool value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Flag)
                        .Write(value);

            return this;
        }

        public IHerculesRecordBuilder Add(string key, float value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Float)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordBuilder Add(string key, double value)
        {
            TagsCount++;

            binaryWriter.WriteWithByteLengthPrefix(key)
                        .Write((byte)TagValueTypeDefinition.Double)
                        .WriteInNetworkByteOrder(value);

            return this;
        }

        public IHerculesRecordBuilder Add(string key, string value)
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

        public IHerculesRecordBuilder Add(string key, byte[] value)
        {
            TagsCount++;

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

        public IHerculesRecordBuilder Add(string key, short[] value)
        {
            TagsCount++;

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

        public IHerculesRecordBuilder Add(string key, int[] value)
        {
            TagsCount++;

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

        public IHerculesRecordBuilder Add(string key, long[] value)
        {
            TagsCount++;

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

        public IHerculesRecordBuilder Add(string key, bool[] value)
        {
            TagsCount++;

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

        public IHerculesRecordBuilder Add(string key, float[] value)
        {
            TagsCount++;

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

        public IHerculesRecordBuilder Add(string key, double[] value)
        {
            TagsCount++;

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

        public IHerculesRecordBuilder Add(string key, string[] value)
        {
            TagsCount++;

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