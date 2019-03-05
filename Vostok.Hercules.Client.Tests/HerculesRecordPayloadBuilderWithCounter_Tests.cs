using System;
using System.Collections;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client.Tests
{
    [TestFixture]
    internal class HerculesRecordPayloadBuilderWithCounter_Tests
    {
        private const int MaxNumberOfTags = ushort.MaxValue;

        [TestCaseSource(nameof(TestCases))]
        public void Should_count_all_kinds_of_write_operations(Action<IHerculesTagsBuilder> add)
        {
            var writer = CreateWriter();
            using (var builder = CreateBuilder(writer))
                add.Invoke(builder);

            GetFieldsCount(writer.Buffer).Should().Be(1);
        }

        [TestCase]
        public void Should_count_write_operations()
        {
            var writer = CreateWriter();
            using (var builder = CreateBuilder(writer))
                builder.AddValue("k1", 1).AddValue("k2", "str").AddValue("k3", 4.0);

            GetFieldsCount(writer.Buffer).Should().Be(3);
        }

        [TestCase]
        public void Should_have_zero_count_when_no_tags_written()
        {
            var writer = CreateWriter();
            CreateBuilder(writer).Dispose();

            GetFieldsCount(writer.Buffer).Should().Be(0);
        }

        [TestCase]
        public void Should_not_throw_when_maximum_number_of_tags_are_written()
        {
            var writer = CreateWriter();
            using (var builder = CreateBuilder(writer))
            {
                for (var i = 0; i < MaxNumberOfTags; i++)
                    builder.AddValue(i.ToString(), 0);
            }

            GetFieldsCount(writer.Buffer).Should().Be(MaxNumberOfTags);
        }

        [TestCase]
        public void Should_throw_OverflowException_when_too_many_tags_are_written()
        {
            var writer = CreateWriter();
            using (var builder = CreateBuilder(writer))
            {
                for (var i = 0; i < MaxNumberOfTags; i++)
                    builder.AddValue(i.ToString(), 0);
                new Action(() => builder.AddValue("overflow", 0)).Should().Throw<OverflowException>();
            }

            GetFieldsCount(writer.Buffer).Should().Be(MaxNumberOfTags);
        }

        private static ushort GetFieldsCount(byte[] buffer)
        {
            var reader = new BinaryBufferReader(buffer, 0) {Endianness = Endianness.Big};
            return reader.ReadUInt16();
        }

        private static IEnumerable TestCases()
        {
            var cases = new (string name, Action<IHerculesTagsBuilder> testCase)[]
            {
                ("Container", builder => builder.AddContainer("key", x => x.AddValue("nested", 0))),
                ("Byte", builder => builder.AddValue("key", (byte)0)),
                ("Int16", builder => builder.AddValue("key", (short)0)),
                ("Int32", builder => builder.AddValue("key", 0)),
                ("Int64", builder => builder.AddValue("key", 0L)),
                ("Bool", builder => builder.AddValue("key", true)),
                ("Float", builder => builder.AddValue("key", 0F)),
                ("Double", builder => builder.AddValue("key", 0D)),
                ("String", builder => builder.AddValue("key", "value")),
                ("Guid", builder => builder.AddValue("key", Guid.Empty)),
                ("ContainerArray", builder => builder.AddVectorOfContainers("key", new Action<IHerculesTagsBuilder>[] {x => x.AddValue("nested", 0)})),
                ("ByteArray", builder => builder.AddVector("key", new[] {(byte)0})),
                ("Int16Array", builder => builder.AddVector("key", new[] {(short)0})),
                ("Int32Array", builder => builder.AddVector("key", new[] {0})),
                ("Int64Array", builder => builder.AddVector("key", new[] {0L})),
                ("BoolArray", builder => builder.AddVector("key", new[] {true})),
                ("FloatArray", builder => builder.AddVector("key", new[] {0F})),
                ("DoubleArray", builder => builder.AddVector("key", new[] {0D})),
                ("StringArray", builder => builder.AddVector("key", new[] {"value"})),
                ("GuidArray", builder => builder.AddVector("key", new[] {Guid.Empty}))
            };

            return cases.Select(x => new TestCaseData(x.testCase).SetName(x.name));
        }

        private static BinaryBufferWriter CreateWriter()
            => new BinaryBufferWriter(0) {Endianness = Endianness.Big};

        private static HerculesRecordPayloadBuilderWithCounter CreateBuilder(IBinaryWriter writer)
            => new HerculesRecordPayloadBuilderWithCounter(writer);
    }
}