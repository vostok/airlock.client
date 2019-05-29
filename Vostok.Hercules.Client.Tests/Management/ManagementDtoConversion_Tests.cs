using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Management;

namespace Vostok.Hercules.Client.Tests.Management
{
    [TestFixture]
    internal class ManagementDtoConversion_Tests
    {
        [Test]
        public void Should_convert_stream_creation_query_to_dto_and_back_to_description(
            [Values(StreamType.Base, StreamType.Derived)]
            StreamType type)
        {
            var query = new CreateStreamQuery("my-stream")
            {
                Type = type,
                TTL = 2.Days(),
                Partitions = 35,
                ShardingKey = new[] {"key"},
                Sources = new[] {"foo", "bar"}
            };

            var dto = StreamDescriptionDtoConverter.CreateFromQuery(query);

            var description = StreamDescriptionDtoConverter.ConvertToDescription(dto);

            description.Should().BeEquivalentTo(query);
        }

        [Test]
        public void Should_convert_timeline_creation_query_to_dto_and_back_to_description()
        {
            var query = new CreateTimelineQuery("my-timeline", new[] {"foo", "bar"})
            {
                Slices = 35,
                TTL = 2.Days(),
                TimetrapSize = 2.Seconds(),
                ShardingKey = new[] {"key"}
            };

            var dto = TimelineDescriptionDtoConverter.CreateFromQuery(query);

            var description = TimelineDescriptionDtoConverter.ConvertToDescription(dto);

            description.Should().BeEquivalentTo(query);
        }
    }
}