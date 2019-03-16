using System;
using FluentAssertions;
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
        public void Should_write_externally_provided_timestamp_by_default()
        {
            TestSerialization(_ => {}).Timestamp.Should().Be(defaultTimestamp);
        }

        private HerculesEvent TestSerialization(Action<IHerculesEventBuilder> build)
        {
            var binaryWriter = new BinaryBufferWriter(16) { Endianness = Endianness.Big };
            var binaryBuilder = new BinaryEventBuilder(binaryWriter, () => defaultTimestamp, Constants.ProtocolVersion);
            var memoryBuilder = new HerculesEventBuilder();

            memoryBuilder.SetTimestamp(defaultTimestamp);

            build(binaryBuilder);
            build(memoryBuilder);

            binaryBuilder.Dispose();

            var binaryReader = new BinaryBufferReader(binaryWriter.Buffer, 0) {Endianness = Endianness.Big};

            var memoryEvent = memoryBuilder.BuildEvent();
            var binaryEvent = BinaryEventReader.ReadEvent(binaryReader);

            binaryEvent.Should().Be(memoryEvent);

            return binaryEvent;
        }
    }
}