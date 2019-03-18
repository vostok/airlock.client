using System;
using System.Collections.Generic;
using Vostok.Commons.Binary;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Serialization.Helpers;

namespace Vostok.Hercules.Client.Serialization.Builders
{
    internal class BinaryEventBuilder : IHerculesEventBuilder, IDisposable
    {
        private readonly IBinaryWriter binaryWriter;
        private readonly Func<DateTimeOffset> timeProvider;

        private readonly long timestampPosition;
        private readonly BinaryCountingTagsBuilder tagsBuilder;

        private DateTimeOffset timestampInternal;

        public BinaryEventBuilder(IBinaryWriter binaryWriter, Func<DateTimeOffset> timeProvider, byte protocolVersion)
        {
            this.binaryWriter = binaryWriter.EnsureBigEndian();
            this.timeProvider = timeProvider;

            binaryWriter.Write(protocolVersion);

            timestampPosition = binaryWriter.Position;
            binaryWriter.Write(0L);
            binaryWriter.Write(Guid.NewGuid());

            tagsBuilder = new BinaryCountingTagsBuilder(binaryWriter);
        }

        public IHerculesEventBuilder SetTimestamp(DateTimeOffset timestamp)
        {
            timestampInternal = timestamp;
            return this;
        }

        public IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> value)
            => tagsBuilder.AddContainer(key, value);

        public IHerculesTagsBuilder AddVectorOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> valueBuilders)
            => tagsBuilder.AddVectorOfContainers(key, valueBuilders);

        public IHerculesTagsBuilder AddNull(string key)
            => tagsBuilder.AddNull(key);

        public IHerculesTagsBuilder AddValue(string key, byte value)
            => tagsBuilder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, short value)
            => tagsBuilder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, int value)
            => tagsBuilder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, long value)
            => tagsBuilder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, bool value)
            => tagsBuilder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, float value)
            => tagsBuilder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, double value)
            => tagsBuilder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, Guid value)
            => tagsBuilder.AddValue(key, value);

        public IHerculesTagsBuilder AddValue(string key, string value)
            => tagsBuilder.AddValue(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<byte> value)
            => tagsBuilder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<short> value)
            => tagsBuilder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<int> value)
            => tagsBuilder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<long> value)
            => tagsBuilder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<bool> value)
            => tagsBuilder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<float> value)
            => tagsBuilder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<double> value)
            => tagsBuilder.AddVector(key, value);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<Guid> values)
            => tagsBuilder.AddVector(key, values);

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<string> value)
            => tagsBuilder.AddVector(key, value);

        public void Dispose()
        {
            tagsBuilder.Dispose();

            var timestamp = timestampInternal != default
                ? timestampInternal
                : timeProvider();

            using (binaryWriter.JumpTo(timestampPosition))
                binaryWriter.Write(EpochHelper.ToUnixTimeUtcTicks(timestamp.UtcDateTime));
        }
    }
}