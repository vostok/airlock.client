using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Serialization.Builders;
using Vostok.Hercules.Client.Serialization.Readers;

namespace Vostok.Hercules.Client.Tests.Serialization
{
    [TestFixture]
    internal class BinaryEventBuilder_Tests
    {
        private DateTimeOffset defaultTimestamp;

        [SetUp]
        public void TestSetup()
        {
            defaultTimestamp = DateTimeOffset.UtcNow;
        }

        [Test]
        public void Should_correctly_serialize_externally_provided_timestamp_by_default()
        {
            TestSerialization(_ => {}).Timestamp.Should().Be(defaultTimestamp);
        }

        [Test]
        public void Should_correctly_serialize_customly_provided_timestamp()
        {
            var timestamp = DateTimeOffset.UtcNow + 1.Hours();

            TestSerialization(builder => builder.SetTimestamp(timestamp));
        }

        [Test]
        public void Should_correctly_serialize_primitive_scalar_values()
        {
            var guidValue = Guid.NewGuid();

            TestSerialization(
                builder =>
                {
                    builder.AddValue("key1", (byte)123);
                    builder.AddValue("key2", (short) 12345);
                    builder.AddValue("key3", 55654645);
                    builder.AddValue("key4", -645645634564356L);
                    builder.AddValue("key5", true);
                    builder.AddValue("key6", false);
                    builder.AddValue("key7", 5345435.434f);
                    builder.AddValue("key8", -15345435.434d);
                    builder.AddValue("key9", "ascii string");
                    builder.AddValue("key10", "кириллическая строка");
                    builder.AddValue("key11", guidValue);
                    builder.AddNull("null");
                });
        }

        [Test]
        public void Should_correctly_serialize_vectors_of_primitive_scalar_values()
        {
            var guidValues = new[] {Guid.NewGuid(), Guid.NewGuid()};
            var byteArray = Guid.NewGuid().ToByteArray();
            
            TestSerialization(
                builder =>
                {
                    builder.AddVector("key1", new [] {byte.MinValue, byte.MaxValue});
                    builder.AddVector("key2", new [] {short.MinValue, short.MaxValue});
                    builder.AddVector("key3", new [] {int.MinValue, int.MaxValue});
                    builder.AddVector("key4", new int[] {});
                    builder.AddVector("key5", byteArray);
                    builder.AddVector("key6", byteArray.ToList());
                    builder.AddVector("key7", new byte[]{});
                    builder.AddVector("key8", new[] { long.MinValue, long.MaxValue });
                    builder.AddVector("key9", new[] { true, false });
                    builder.AddVector("key10", new[] { float.MaxValue, float.MinValue, float.PositiveInfinity });
                    builder.AddVector("key11", new[] { double.MaxValue, double.MinValue, double.PositiveInfinity });
                    builder.AddVector("key12", new[] { "foo", "bar", "baz", "longer string" });
                    builder.AddVector("key12", guidValues);
                });
        }

        [Test]
        public void Should_correctly_serialize_nested_containers()
        {
            TestSerialization(
                builder =>
                {
                    builder.AddContainer("empty", b => {});
                   
                    builder.AddContainer("foo",
                        b =>
                        {
                            builder.AddValue("key", "value");
                            builder.AddContainer("baz", b2 => { b2.AddValue("int", 123); });

                        });

                    builder.AddContainer("bar",
                        b =>
                        {
                            builder.AddValue("key", "value");
                            builder.AddContainer("baz", b2 => { b2.AddValue("long", 343543353L); });
                        });
                });
        }

        [Test]
        public void Should_correctly_serialize_vectors_of_containers()
        {
            TestSerialization(
                builder =>
                {
                    builder.AddVectorOfContainers("containers", new Action<IHerculesTagsBuilder>[]
                    {
                        b => b.AddValue("k1", "v1"),
                        b => b.AddValue("k2", "v2"),
                        b => b.AddValue("k3", "v3")
                    });
                });
        }

        private HerculesEvent TestSerialization(Action<IHerculesEventBuilder> build)
        {
            var memoryBuilder = new HerculesEventBuilder();

            memoryBuilder.SetTimestamp(defaultTimestamp);

            build(memoryBuilder);

            var memoryEvent = memoryBuilder.BuildEvent();

            var binaryWriter = new BinaryBufferWriter(16) { Endianness = Endianness.Big };

            for (var i = 0; i < 3; i++)
            {
                using (var binaryBuilder = new BinaryEventBuilder(binaryWriter, () => defaultTimestamp, Constants.EventProtocolVersion))
                {
                    build(binaryBuilder);
                }
            }

            var binaryReader = new BinaryBufferReader(binaryWriter.Buffer, 0) {Endianness = Endianness.Big};

            var binaryEvent1 = BinaryEventReader.ReadEvent(binaryReader);
            var binaryEvent2 = BinaryEventReader.ReadEvent(binaryReader);
            var binaryEvent3 = BinaryEventReader.ReadEvent(binaryReader);

            binaryEvent1.Should().Be(memoryEvent);
            binaryEvent2.Should().Be(memoryEvent);
            binaryEvent3.Should().Be(memoryEvent);

            return binaryEvent3;
        }
    }
}