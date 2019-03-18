using System;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Serialization.Readers;
using Vostok.Hercules.Client.Serialization.Writers;

namespace Vostok.Hercules.Client.Tests.Serialization
{
    [TestFixture]
    internal class TimelineCoordinatesSerialization_Tests
    {
        [Test]
        public void Should_serialize_and_deserialize_empty_coordinates()
        {
            TestSerialization(TimelineCoordinates.Empty);
        }

        [Test]
        public void Should_serialize_and_deserialize_nontrivial_coordinates()
        {
            var coordinates = new TimelineCoordinates(new[]
            {
                new TimelinePosition { Slice = 0, Offset = 4325L, EventId = GenerateEventId()},
                new TimelinePosition { Slice = 1, Offset = 645645L, EventId = GenerateEventId()},
                new TimelinePosition { Slice = 2, Offset = 155L, EventId = GenerateEventId()},
                new TimelinePosition { Slice = 3, Offset = 5464, EventId = GenerateEventId()}
            });

            TestSerialization(coordinates);
        }

        private static void TestSerialization(TimelineCoordinates coordinates)
        {
            var writer = new BinaryBufferWriter(1) { Endianness = Endianness.Big };

            TimelineCoordinatesWriter.Write(coordinates, writer);

            var reader = new BinaryBufferReader(writer.Buffer, 0) { Endianness = Endianness.Big };

            var deserialized = TimelineCoordinatesReader.Read(reader);

            deserialized.Positions.Length.Should().Be(coordinates.Positions.Length);

            for (var i = 0; i < deserialized.Positions.Length; i++)
            {
                deserialized.Positions[i].Slice.Should().Be(coordinates.Positions[i].Slice);
                deserialized.Positions[i].Offset.Should().Be(coordinates.Positions[i].Offset);
                deserialized.Positions[i].EventId.Should().Equal(coordinates.Positions[i].EventId);
            }
        }

        private static byte[] GenerateEventId()
        {
            var id = new byte[24];

            new Random(Guid.NewGuid().GetHashCode()).NextBytes(id);

            return id;
        }
    }
}