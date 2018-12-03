using System;
using System.Collections.Generic;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Binary;
using Vostok.Hercules.Client.TimeBasedUuid;

namespace Vostok.Hercules.Client
{
    internal class HerculesEventBuilder : IHerculesEventBuilder, IDisposable
    {
        private readonly IBinaryWriter binaryWriter;
        private readonly ITimeGuidGenerator timeGuidGenerator;
        private readonly int timeGuidPosition;
        private readonly HerculesRecordPayloadBuilderWithCounter builder;

        private DateTimeOffset timestampInternal;

        public HerculesEventBuilder(IBinaryWriter binaryWriter, ITimeGuidGenerator timeGuidGenerator)
        {
            this.binaryWriter = binaryWriter;
            this.timeGuidGenerator = timeGuidGenerator;

            timeGuidPosition = binaryWriter.Position;
            binaryWriter.Write(TimeGuid.Empty);

            builder = new HerculesRecordPayloadBuilderWithCounter(binaryWriter);
        }

        public IHerculesEventBuilder SetTimestamp(DateTimeOffset timestamp)
        {
            timestampInternal = timestamp;
            return this;
        }

        public IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> value)
        {
            return builder.AddContainer(key, value);
        }

        public IHerculesTagsBuilder AddArrayOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> valueBuilders) =>
            throw new NotImplementedException();

        public IHerculesTagsBuilder AddValue(string key, byte value)
        {
            return builder.AddValue(key, value);
        }

        public IHerculesTagsBuilder AddValue(string key, short value)
        {
            return builder.AddValue(key, value);
        }

        public IHerculesTagsBuilder AddValue(string key, int value)
        {
            return builder.AddValue(key, value);
        }

        public IHerculesTagsBuilder AddValue(string key, long value)
        {
            return builder.AddValue(key, value);
        }

        public IHerculesTagsBuilder AddValue(string key, bool value)
        {
            return builder.AddValue(key, value);
        }

        public IHerculesTagsBuilder AddValue(string key, float value)
        {
            return builder.AddValue(key, value);
        }

        public IHerculesTagsBuilder AddValue(string key, double value)
        {
            return builder.AddValue(key, value);
        }

        public IHerculesTagsBuilder AddValue(string key, Guid value)
            => throw new NotImplementedException();

        public IHerculesTagsBuilder AddValue(string key, string value)
        {
            return builder.AddValue(key, value);
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<byte> value)
        {
            return builder.AddArray(key, value);
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<short> value)
        {
            return builder.AddArray(key, value);
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<int> value)
        {
            return builder.AddArray(key, value);
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<long> value)
        {
            return builder.AddArray(key, value);
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<bool> value)
        {
            return builder.AddArray(key, value);
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<float> value)
        {
            return builder.AddArray(key, value);
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<double> value)
        {
            return builder.AddArray(key, value);
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<Guid> values) =>
            throw new NotImplementedException();

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<string> value)
        {
            return builder.AddArray(key, value);
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