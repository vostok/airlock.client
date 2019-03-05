using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client.Sink.Writing
{
    internal class HerculesRecordPayloadBuilderWithCounter : IHerculesTagsBuilder, IDisposable
    {
        private readonly IBinaryWriter binaryWriter;
        private readonly long countPosition;
        private readonly HerculesRecordPayloadBuilder builder;

        private ushort counter;

        public HerculesRecordPayloadBuilderWithCounter(IBinaryWriter binaryWriter)
        {
            this.binaryWriter = binaryWriter;

            countPosition = binaryWriter.Position;
            binaryWriter.Write((ushort)0);

            builder = new HerculesRecordPayloadBuilder(binaryWriter);
        }

        public IHerculesTagsBuilder AddContainer(string key, Action<IHerculesTagsBuilder> value)
        {
            IncrementCounter();
            builder.AddContainer(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, byte value)
        {
            IncrementCounter();
            builder.AddValue(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, short value)
        {
            IncrementCounter();
            builder.AddValue(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, int value)
        {
            IncrementCounter();
            builder.AddValue(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, long value)
        {
            IncrementCounter();
            builder.AddValue(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, bool value)
        {
            IncrementCounter();
            builder.AddValue(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, float value)
        {
            IncrementCounter();
            builder.AddValue(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, double value)
        {
            IncrementCounter();
            builder.AddValue(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, Guid value)
        {
            IncrementCounter();
            builder.AddValue(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddValue(string key, string value)
        {
            IncrementCounter();
            builder.AddValue(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddVectorOfContainers(string key, IReadOnlyList<Action<IHerculesTagsBuilder>> value)
        {
            IncrementCounter();
            builder.AddVectorOfContainers(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddNull(string key)
        {
            IncrementCounter();
            builder.AddNull(key);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<byte> value)
        {
            IncrementCounter();
            builder.AddVector(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<short> value)
        {
            IncrementCounter();
            builder.AddVector(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<int> value)
        {
            IncrementCounter();
            builder.AddVector(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<long> value)
        {
            IncrementCounter();
            builder.AddVector(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<bool> value)
        {
            IncrementCounter();
            builder.AddVector(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<float> value)
        {
            IncrementCounter();
            builder.AddVector(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<double> value)
        {
            IncrementCounter();
            builder.AddVector(key, value);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<Guid> values)
        {
            IncrementCounter();
            builder.AddVector(key, values);

            return this;
        }

        public IHerculesTagsBuilder AddVector(string key, IReadOnlyList<string> value)
        {
            IncrementCounter();
            builder.AddVector(key, value);

            return this;
        }

        public void Dispose()
        {
            using (binaryWriter.JumpTo(countPosition))
                binaryWriter.Write(counter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void IncrementCounter()
        {
            checked
            {
                ++counter;
            }
        }
    }
}