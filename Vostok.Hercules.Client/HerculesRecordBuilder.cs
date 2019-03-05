using System;
using System.Collections.Generic;
using Vostok.Commons.Binary;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client
{
    internal class HerculesEventBuilder : IHerculesEventBuilder, IDisposable
    {
        private readonly IBinaryWriter binaryWriter;
        private readonly Func<DateTimeOffset> timeProvider;
        private readonly long timestampPosition;
        private readonly HerculesRecordPayloadBuilderWithCounter builder;

        private DateTimeOffset timestampInternal;

        public HerculesEventBuilder(IBinaryWriter binaryWriter, Func<DateTimeOffset> timeProvider)
        {
            this.binaryWriter = binaryWriter;
            this.timeProvider = timeProvider;

            timestampPosition = binaryWriter.Position;
            binaryWriter.Write(0L);
            binaryWriter.Write(Guid.NewGuid());

            builder = new HerculesRecordPayloadBuilderWithCounter(binaryWriter);
        }

        public IHerculesEventBuilder SetTimestamp(DateTimeOffset timestamp)
        {
            timestampInternal = timestamp;
            return this;
        }

        public IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> value)
            => builder.AddContainer(key, value);

        public IHerculesTagsBuilder AddVectorOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> valueBuilders)
            => builder.AddVectorOfContainers(key, valueBuilders);

        public IHerculesTagsBuilder AddNull(string key)
            => builder.AddNull(key);

        public IHerculesTagsBuilder AddValue(string key, byte value)
            => builder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, short value)
            => builder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, int value)
            => builder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, long value)
            => builder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, bool value)
            => builder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, float value)
            => builder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, double value)
            => builder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, Guid value)
            => builder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, string value)
            => builder.AddValue(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<byte> value)
            => builder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<short> value)
            => builder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<int> value)
            => builder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<long> value)
            => builder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<bool> value)
            => builder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<float> value)
            => builder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<double> value)
            => builder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<Guid> values)
            => builder.AddVector(key, values);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<string> value)
            => builder.AddVector(key, value);

        public void Dispose()
        {
            builder.Dispose();

            var timestamp = timestampInternal != default
                ? timestampInternal
                : timeProvider();

            using (binaryWriter.JumpTo(timestampPosition))
                binaryWriter.Write(EpochHelper.ToUnixTimeUtcTicks(timestamp.UtcDateTime));
        }
    }
}