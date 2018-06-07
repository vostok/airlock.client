using System;
using System.Linq;
using System.Text;
using Vostok.Airlock.Client.Abstractions;
using Vostok.Commons.Binary;

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

        public IAirlockRecordBuilder SetTimestamp(DateTimeOffset timestamp)
        {
            Timestamp = timestamp.ToUniversalTime().ToUnixTimeNanoseconds();

            return this;
        }

        public IAirlockRecordBuilder Add(string key, byte value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueTypeDefinition.Byte);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, short value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueTypeDefinition.Short);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, int value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueTypeDefinition.Integer);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, long value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueTypeDefinition.Long);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, bool value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueTypeDefinition.Flag);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, float value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueTypeDefinition.Float);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, double value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueTypeDefinition.Double);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, string value)
        {
            binaryWriter.Write(key);

            if (value.Length > 255)
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.Text);
                binaryWriter.Write(value);
            }
            else
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.String);

                binaryWriter.Write((byte) Encoding.UTF8.GetByteCount(value));
                binaryWriter.WriteWithoutLengthPrefix(value);
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, byte[] values)
        {
            binaryWriter.Write(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ByteArray);
                binaryWriter.WriteCollection(values, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte)TagValueTypeDefinition.ByteVector);
                binaryWriter.WriteVector(values, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, short[] values)
        {
            binaryWriter.Write(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.ShortArray);
                binaryWriter.WriteCollection(values, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.ShortVector);
                binaryWriter.WriteVector(values, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, int[] values)
        {
            binaryWriter.Write(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.IntegerArray);
                binaryWriter.WriteCollection(values, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.IntegerVector);
                binaryWriter.WriteVector(values, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, long[] values)
        {
            binaryWriter.Write(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.LongArray);
                binaryWriter.WriteCollection(values, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.LongVector);
                binaryWriter.WriteVector(values, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, bool[] values)
        {
            binaryWriter.Write(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.FlagArray);
                binaryWriter.WriteCollection(values, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.FlagVector);
                binaryWriter.WriteVector(values, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, float[] values)
        {
            binaryWriter.Write(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.FloatArray);
                binaryWriter.WriteCollection(values, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.FloatVector);
                binaryWriter.WriteVector(values, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, double[] values)
        {
            binaryWriter.Write(key);

            if (values.Length > 255)
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.DoubleArray);
                binaryWriter.WriteCollection(values, (writer, item) => writer.Write(item));
            }
            else
            {
                binaryWriter.Write((byte) TagValueTypeDefinition.DoubleVector);
                binaryWriter.WriteVector(values, (writer, item) => writer.Write(item));
            }

            return this;
        }

        public IAirlockRecordBuilder Add(string key, string[] values)
        {
            binaryWriter.Write(key);

            if (values.Length > 255)
            {
                if (values.Any(x => x.Length > 255))
                {
                    binaryWriter.Write((byte) TagValueTypeDefinition.TextArray);

                    binaryWriter.WriteCollection(values, (writer, item) => writer.Write(item));
                }
                else
                {
                    binaryWriter.Write((byte) TagValueTypeDefinition.StringArray);

                    binaryWriter.WriteCollection(values, (writer, item) =>
                    {
                        writer.Write((byte) Encoding.UTF8.GetByteCount(item));
                        writer.WriteWithoutLengthPrefix(item);
                    });
                }
            }
            else
            {
                if (values.Any(x => x.Length > 255))
                {
                    binaryWriter.Write((byte) TagValueTypeDefinition.TextVector);

                    binaryWriter.WriteVector(values, (writer, item) => writer.Write(item));
                }
                else
                {
                    binaryWriter.Write((byte) TagValueTypeDefinition.StringVector);

                    binaryWriter.WriteVector(values, (writer, item) =>
                    {
                        writer.Write((byte) Encoding.UTF8.GetByteCount(item));
                        writer.WriteWithoutLengthPrefix(item);
                    });
                }
            }

            return this;
        }
    }
}