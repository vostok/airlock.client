using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Serialization.Readers;
using Vostok.Hercules.Client.Serialization.Writers;

namespace Vostok.Hercules.Client.Tests.Serialization
{
    [TestFixture]
    internal class StreamCoordinatesSerialization_Tests
    {
        [Test]
        public void Should_serialize_and_deserialize_empty_coordinates()
        {
            TestSerialization(StreamCoordinates.Empty);
        }

        [Test]
        public void Should_serialize_and_deserialize_nontrivial_coordinates()
        {
            var coordinates = new StreamCoordinates(new []
            {
                new StreamPosition { Partition = 0, Offset = 4325L}, 
                new StreamPosition { Partition = 1, Offset = 645645L}, 
                new StreamPosition { Partition = 2, Offset = 155L}, 
                new StreamPosition { Partition = 3, Offset = 5464L} 
            });

            TestSerialization(coordinates);
        }

        private static void TestSerialization(StreamCoordinates coordinates)
        {
            var writer = new BinaryBufferWriter(1) {Endianness = Endianness.Big};

            StreamCoordinatesWriter.Write(coordinates, writer);

            var reader = new BinaryBufferReader(writer.Buffer, 0) {Endianness = Endianness.Big};

            StreamCoordinatesReader.Read(reader).Should().BeEquivalentTo(coordinates);
        }
    }
}