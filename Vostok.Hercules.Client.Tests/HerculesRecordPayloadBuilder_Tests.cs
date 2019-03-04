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
    internal class HerculesRecordPayloadBuilder_Tests
    {
        [TestCaseSource(nameof(TestCases))]
        public TagType Should_write_correct_tag_type(Action<IHerculesTagsBuilder> action)
        {
            const string key = "key";
            
            var writer = CreateWriter();
            
            var builder = CreateBuilder(writer);
            action.Invoke(builder);

            return (TagType)writer.Buffer[key.Length + 1];
        }

        [TestCaseSource(nameof(VectorTestCases))]
        public TagType Should_write_correct_tag_type_for_vector(Action<IHerculesTagsBuilder> action)
        {
            const string key = "key";
            
            var writer = CreateWriter();
            
            var builder = CreateBuilder(writer);
            action.Invoke(builder);

            writer.Buffer[key.Length + 1].Should().Be((byte)TagType.Vector);
            return (TagType)writer.Buffer[key.Length + 2];
        }

        private static IEnumerable TestCases()
        {
            var cases = new (TagType type, Action<IHerculesTagsBuilder> testCase)[]
            {
                (TagType.Container, builder => builder.AddContainer("key", x => x.AddValue("nested", 0))),
                (TagType.Byte, builder => builder.AddValue("key", (byte)0)),
                (TagType.Short, builder => builder.AddValue("key", (short)0)),
                (TagType.Integer, builder => builder.AddValue("key", 0)),
                (TagType.Long, builder => builder.AddValue("key", 0L)),
                (TagType.Flag, builder => builder.AddValue("key", true)),
                (TagType.Float, builder => builder.AddValue("key", 0F)),
                (TagType.Double, builder => builder.AddValue("key", 0D)),
                (TagType.String, builder => builder.AddValue("key", "value")),
                (TagType.UUID, builder => builder.AddValue("key", Guid.Empty)),
            };

            return cases.Select(x => new TestCaseData(x.testCase).SetName(x.type.ToString()).Returns(x.type));
        }
        
        private static IEnumerable VectorTestCases()
        {
            var cases = new (TagType type, Action<IHerculesTagsBuilder> testCase)[]
            {
                (TagType.Container, builder => builder.AddVectorOfContainers("key", new Action<IHerculesTagsBuilder>[] {x => x.AddValue("nested", 0)})),
                (TagType.Byte, builder => builder.AddVector("key", new[] {(byte)0})),
                (TagType.Short, builder => builder.AddVector("key", new[] {(short)0})),
                (TagType.Integer, builder => builder.AddVector("key", new[] {0})),
                (TagType.Long, builder => builder.AddVector("key", new[] {0L})),
                (TagType.Flag, builder => builder.AddVector("key", new[] {true})),
                (TagType.Float, builder => builder.AddVector("key", new[] {0F})),
                (TagType.Double, builder => builder.AddVector("key", new[] {0D})),
                (TagType.String, builder => builder.AddVector("key", new[] {"value"})),
                (TagType.UUID, builder => builder.AddVector("key", new[] {Guid.Empty})),
            };

            return cases.Select(x => new TestCaseData(x.testCase).SetName(x.type + "Vector").Returns(x.type));
        }
        
        private static BinaryBufferWriter CreateWriter() => new BinaryBufferWriter(0){Endianness = Endianness.Big};

        private static HerculesRecordPayloadBuilder CreateBuilder(IBinaryWriter writer) => new HerculesRecordPayloadBuilder(writer);
    }
}