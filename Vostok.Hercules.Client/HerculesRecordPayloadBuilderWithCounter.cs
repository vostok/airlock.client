using System;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordPayloadBuilderWithCounter : IHerculesRecordPayloadBuilder, IDisposable
    {
        private readonly IBinaryWriter binaryWriter;
        private readonly int countPosition;
        private readonly HerculesRecordPayloadBuilder builder;

        private short counter;

        public HerculesRecordPayloadBuilderWithCounter(IBinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;

            countPosition = binaryWriter.Position;
            binaryWriter.WriteInNetworkByteOrder((short)0);

            builder = new HerculesRecordPayloadBuilder(binaryWriter);
        }

        public IHerculesRecordPayloadBuilder Add(string key, Func<IHerculesRecordPayloadBuilder, IHerculesRecordPayloadBuilder> value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, byte value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, short value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, int value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, long value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, bool value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, float value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, double value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, string value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, Func<IHerculesRecordPayloadBuilder, IHerculesRecordPayloadBuilder>[] value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, byte[] value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, short[] value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, int[] value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, long[] value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, bool[] value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, float[] value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, double[] value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, string[] value)
        {
            builder.Add(key, value);
            counter++;
            return this;
        }

        public void Dispose()
        {
            var currentPosition = binaryWriter.Position;

            binaryWriter.Position = countPosition;
            binaryWriter.WriteInNetworkByteOrder(counter);

            binaryWriter.Position = currentPosition;
        }
    }
}