using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client
{
    internal class HerculesRecordPayloadBuilder : IHerculesTagsBuilder
    {
        private readonly IHerculesBinaryWriter writer;

        public HerculesRecordPayloadBuilder(IHerculesBinaryWriter writer)
        {
            this.writer = writer;
        }

        public IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Container);

            using (var builder = new HerculesRecordPayloadBuilderWithCounter(writer))
                value.Invoke(builder);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, byte value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Byte);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, short value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Short);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, int value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Integer);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, long value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Long);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, bool value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Byte);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, float value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Float);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, double value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Double);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, Guid value)
        { 
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.UUID);
            writer.Write(value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, string value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.String);
            writer.WriteWithLength(value);

            return this;
        }

        public IHerculesTagsBuilder AddVectorOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> values)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.Container);
            writer.Write(values.Count);

            foreach (var action in values)
                using (var builder = new HerculesRecordPayloadBuilderWithCounter(writer))
                    action(builder);

            return this;
        }

        public IHerculesTagsBuilder AddNull(string key)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Null);

            return this;
        }

        private IHerculesTagsBuilder AddVector(string key, byte[] values)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.Byte);
            writer.WriteWithLength(values, 0, values.Length);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<byte> values)
        {
            if (values is byte[] array)
                return AddVector(key, array);

            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.Byte);
            writer.WriteCollection(values, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<short> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.Short);
            writer.WriteCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<int> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.Integer);
            writer.WriteCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<long> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.Long);
            writer.WriteCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<bool> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.Byte);
            writer.WriteCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<float> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.Float);
            writer.WriteCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<double> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.Double);
            writer.WriteCollection(value, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<Guid> values)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.UUID);
            writer.WriteCollection(values, (w, x) => w.Write(x));

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<string> value)
        {
            writer.WriteWithByteLength(key);
            writer.Write(TagValueTypeDefinition.Vector);
            writer.Write(TagValueTypeDefinition.String);
            writer.WriteCollection(value, (w, x) => w.WriteWithLength(x));
            
            return this;
        }
    }
}