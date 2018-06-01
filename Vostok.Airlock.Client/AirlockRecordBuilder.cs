using System;
using Vostok.Airlock.Client.Abstractions;
using Vostok.Commons.Binary;

namespace Vostok.Airlock.Client
{
    public class AirlockRecordBuilder : IAirlockRecordBuilder
    {
        private readonly IBinaryWriter binaryWriter;

        public AirlockRecordBuilder(IBinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;
        }

        internal long Timestamp { get; private set; }

        public IAirlockRecordBuilder SetTimestamp(DateTimeOffset timestamp)
        {
            Timestamp = timestamp.ToUniversalTime().ToUnixTimeMilliseconds();

            return this;
        }

        public IAirlockRecordBuilder Add(string key, byte value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.Byte);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, short value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.Short);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, int value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.Int);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, long value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.Long);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, bool value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.Bool);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, float value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.Float);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, double value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.Double);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, string value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.String);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, byte[] value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.ByteArray);
            binaryWriter.Write(value);

            return this;
        }

        public IAirlockRecordBuilder Add(string key, short[] value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.ShortArray);
            binaryWriter.WriteCollection(value, (writer, item) => writer.Write(item));

            return this;
        }

        public IAirlockRecordBuilder Add(string key, int[] value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.IntArray);
            binaryWriter.WriteCollection(value, (writer, item) => writer.Write(item));

            return this;
        }

        public IAirlockRecordBuilder Add(string key, long[] value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.LongArray);
            binaryWriter.WriteCollection(value, (writer, item) => writer.Write(item));

            return this;
        }

        public IAirlockRecordBuilder Add(string key, bool[] value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.BoolArray);
            binaryWriter.WriteCollection(value, (writer, item) => writer.Write(item));

            return this;
        }

        public IAirlockRecordBuilder Add(string key, float[] value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.FloatArray);
            binaryWriter.WriteCollection(value, (writer, item) => writer.Write(item));

            return this;
        }

        public IAirlockRecordBuilder Add(string key, double[] value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.DoubleArray);
            binaryWriter.WriteCollection(value, (writer, item) => writer.Write(item));

            return this;
        }

        public IAirlockRecordBuilder Add(string key, string[] value)
        {
            binaryWriter.Write(key);
            binaryWriter.Write((byte) TagValueType.StringArray);
            binaryWriter.WriteCollection(value, (writer, item) => writer.Write(item));

            return this;
        }
    }
}