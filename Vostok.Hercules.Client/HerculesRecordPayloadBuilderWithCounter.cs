using System;
using System.Collections.Generic;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordPayloadBuilderWithCounter : IHerculesTagsBuilder, IDisposable
    {
        private readonly IBinaryWriter binaryWriter;
        private readonly int countPosition;
        private readonly HerculesRecordPayloadBuilder builder;

        private short counter;

        public HerculesRecordPayloadBuilderWithCounter(IBinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;

            countPosition = binaryWriter.Position;
            binaryWriter.WriteInNetworkByteOrder((short) 0);

            builder = new HerculesRecordPayloadBuilder(binaryWriter);
        }

        public IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> value)
        {
            builder.AddContainer(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, byte value)
        {
            builder.AddValue(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, short value)
        {
            builder.AddValue(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, int value)
        {
            builder.AddValue(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, long value)
        {
            builder.AddValue(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, bool value)
        {
            builder.AddValue(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, float value)
        {
            builder.AddValue(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, double value)
        {
            builder.AddValue(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, Guid value) =>
            throw new NotImplementedException();

        public IHerculesTagsBuilder AddValue(string key, string value)
        {
            builder.AddValue(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddArrayOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> value)
        {
            builder.AddArrayOfContainers(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<byte> value)
        {
            builder.AddArray(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<short> value)
        {
            builder.AddArray(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<int> value)
        {
            builder.AddArray(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<long> value)
        {
            builder.AddArray(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<bool> value)
        {
            builder.AddArray(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<float> value)
        {
            builder.AddArray(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<double> value)
        {
            builder.AddArray(key, value);
            counter++;
            return this;
        }

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<Guid> values) =>
            throw new NotImplementedException();

        public IHerculesTagsBuilder AddArray(string key, IReadOnlyList<string> value)
        {
            builder.AddArray(key, value);
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