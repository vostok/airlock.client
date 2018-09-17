using System;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Binary;
using Vostok.Hercules.Client.TimeBasedUuid;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordBuilder : IHerculesRecordBuilder, IDisposable
    {
        private readonly IBinaryWriter binaryWriter;
        private readonly ITimeGuidGenerator timeGuidGenerator;
        private readonly int timeGuidPosition;
        private readonly HerculesRecordPayloadBuilderWithCounter builder;

        private DateTimeOffset timestampInternal;

        public HerculesRecordBuilder(IBinaryWriter binaryWriter, ITimeGuidGenerator timeGuidGenerator)
        {
            this.binaryWriter = binaryWriter;
            this.timeGuidGenerator = timeGuidGenerator;

            timeGuidPosition = binaryWriter.Position;
            binaryWriter.Write(TimeGuid.Empty);

            builder = new HerculesRecordPayloadBuilderWithCounter(binaryWriter);
        }

        public IHerculesRecordBuilder SetTimestamp(DateTimeOffset timestamp)
        {
            timestampInternal = timestamp;
            return this;
        }

        public IHerculesRecordPayloadBuilder Add(string key, Func<IHerculesRecordPayloadBuilder, IHerculesRecordPayloadBuilder> value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, byte value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, short value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, int value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, long value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, bool value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, float value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, double value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, string value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, Func<IHerculesRecordPayloadBuilder, IHerculesRecordPayloadBuilder>[] value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, byte[] value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, short[] value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, int[] value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, long[] value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, bool[] value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, float[] value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, double[] value)
        {
            return builder.Add(key, value);
        }

        public IHerculesRecordPayloadBuilder Add(string key, string[] value)
        {
            return builder.Add(key, value);
        }

        public void Dispose()
        {
            builder.Dispose();

            var currentPosition = binaryWriter.Position;

            binaryWriter.Position = timeGuidPosition;

            var timeGuid = timestampInternal != default
                ? timeGuidGenerator.NewGuid(timestampInternal.ToUniversalTime().ToUnixTimeNanoseconds())
                : timeGuidGenerator.NewGuid();
            binaryWriter.Write(timeGuid);

            binaryWriter.Position = currentPosition;
        }
    }
}