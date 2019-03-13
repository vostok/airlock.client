using System;
using System.Collections.Generic;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Sink.Helpers;

namespace Vostok.Hercules.Client.Sink.Writing
{
    internal class RecordPayloadBuilder : IHerculesTagsBuilder
    {
        private readonly IBinaryWriter writer;

        public RecordPayloadBuilder(IBinaryWriter writer)
        {
            this.writer = writer;
        }

        public IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Container);

            using (var builder = new RecordPayloadBuilderWithCounter(writer))
                value.Invoke(builder);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, byte value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Byte);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, short value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Short);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, int value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Integer);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, long value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Long);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, bool value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Flag);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, float value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Float);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, double value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Double);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, Guid value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Uuid);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, string value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.String);
            writer.WriteWithLength(value);

            return this;
        }

        public IHerculesTagsBuilder AddVectorOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> values)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Container);
            writer.Write(values.Count);

            foreach (var action in values)
                using (var builder = new RecordPayloadBuilderWithCounter(writer))
                    action(builder);

            return this;
        }

        public IHerculesTagsBuilder AddNull(string key)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Null);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<byte> values)
        {
            if (values is byte[] array)
                return AddVector(key, array);

            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Byte);
            writer.WriteReadOnlyCollection(values, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<short> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Short);
            writer.WriteReadOnlyCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<int> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Integer);
            writer.WriteReadOnlyCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<long> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Long);
            writer.WriteReadOnlyCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<bool> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Flag);
            writer.WriteReadOnlyCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<float> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Float);
            writer.WriteReadOnlyCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<double> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Double);
            writer.WriteReadOnlyCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<Guid> values)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Uuid);
            writer.WriteReadOnlyCollection(values, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<string> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.String);
            writer.WriteReadOnlyCollection(value, (w, x) => w.WriteWithLength(x));

            return this;
        }

        private IHerculesTagsBuilder AddVector(string key, byte[] values)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagType.Vector);
            writer.Write(TagType.Byte);
            writer.WriteWithLength(values, 0, values.Length);

            return this;
        }
    }
}