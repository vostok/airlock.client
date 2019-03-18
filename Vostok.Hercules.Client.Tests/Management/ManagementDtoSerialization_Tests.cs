using System.IO;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Hercules.Client.Management;
using Vostok.Hercules.Client.Serialization.Json;

namespace Vostok.Hercules.Client.Tests.Management
{
    [TestFixture]
    internal class ManagementDtoSerialization_Tests
    {
        private readonly JsonSerializer serializer = new JsonSerializer();

        [Test]
        public void Should_serialize_empty_stream_description()
        {
            TestSerialization(new StreamDescriptionDto());
        }

        [Test]
        public void Should_serialize_empty_timeline_description()
        {
            TestSerialization(new TimelineDescriptionDto());
        }

        [Test]
        public void Should_serialize_filled_stream_description()
        {
            var description = new StreamDescriptionDto
            {
                Name = "my-stream",
                Partitions = 10,
                Sources = new[] { "foo", "bar" },
                ShardingKey = new[] { "baz" },
                TtlMilliseconds = 453534,
                Type = "derived"
            };

            TestSerialization(description);
        }

        [Test]
        public void Should_serialize_filled_timeline_description()
        {
            var description = new TimelineDescriptionDto
            {
                Name = "my-timeline",
                Slices = 10,
                Streams = new[] { "foo", "bar" },
                ShardingKey = new[] { "baz" },
                TtlMilliseconds = 453534,
                TimetrapSizeMilliseconds = 5433
            };

            TestSerialization(description);
        }

        private void TestSerialization<T>(T item)
        {
            var serialized = serializer.Serialize(item);

            var deserialized = serializer.Deserialize<T>(new MemoryStream(serialized));

            deserialized.Should().BeEquivalentTo(item);
        }
    }
}