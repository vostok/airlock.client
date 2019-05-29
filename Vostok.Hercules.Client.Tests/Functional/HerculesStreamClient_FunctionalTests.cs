using System;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Tests.Functional.Helpers;

namespace Vostok.Hercules.Client.Tests.Functional
{
    internal class HerculesStreamClient_FunctionalTests
    {
        private static readonly TimeSpan Timeout = 20.Seconds();

        private Helpers.Hercules hercules;

        [SetUp]
        public void Setup()
        {
            if (hercules == null)
                hercules = new Helpers.Hercules();
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            hercules.Dispose();
        }

        [TestCase(50000, 1)]
        [TestCase(50000, 3)]
        public void Should_read_events_in_many_client_shards(int count, int clientShards)
        {
            var builders = TestHelpers.GenerateEventBuilders(count);

            using (hercules.Management.CreateTemporaryStream(out var stream))
            {
                hercules.Gate.Insert(new InsertEventsQuery(stream, builders.ToEvents()), Timeout);

                var shards = hercules.Stream.ReadEvents(stream, count, count / 10, clientShards);

                shards.Should().HaveCount(clientShards);

                shards.SelectMany(x => x).ShouldBeEqual(builders.ToEvents());

                foreach (var shard in shards)
                    shard.Should().NotBeEmpty();
            }
        }

        [TestCase(50000, 3, 1)]
        [TestCase(50000, 3, 3)]
        [TestCase(50000, 20, 2)]
        [TestCase(50000, 20, 20)]
        public void Gate_should_split_events_to_shards_by_shardingKey(int count, int partitions, int clientShards)
        {
            var random = new Random();

            var events = TestHelpers.GenerateEventBuilders(count, b => b.AddValue("shard", random.Next(0, 30))).ToEvents();

            using (hercules.Management.CreateTemporaryStream(out var stream, partitions, new[] {"shard"}))
            {
                hercules.Gate.Insert(new InsertEventsQuery(stream, events), Timeout);

                var shards = hercules.Stream.ReadEvents(stream, count, count / 10, clientShards);

                shards.SelectMany(x => x).ShouldBeEqual(events);

                shards.Should().HaveCount(clientShards);

                var shardingKeyValues = shards
                    .Select(
                        s => s
                            .Select(x => x.Tags["shard"].AsInt)
                            .Distinct()
                            .ToArray())
                    .ToArray();

                for (var i = 0; i < shardingKeyValues.Length; ++i)
                for (var j = i + 1; j < shardingKeyValues.Length; ++j)
                    shardingKeyValues[i].Should().NotIntersectWith(shardingKeyValues[j]);
            }
        }

        [Test]
        public void SeekToEnd_should_skip_existing_events()
        {
            var events = TestHelpers.GenerateEventBuilders(100).ToEvents();
            var part1 = events.Take(30).ToList();
            var part2 = events.Skip(30).ToList();

            using (hercules.Management.CreateTemporaryStream(out var stream))
            {
                hercules.Gate.Insert(new InsertEventsQuery(stream, part1), Timeout);

                hercules.Stream.ReadEvents(stream, 30).ShouldBeEqual(part1);

                var end = hercules.Stream.SeekToEnd(new SeekToEndStreamQuery(stream), Timeout);
                end.EnsureSuccess();

                hercules.Gate.Insert(new InsertEventsQuery(stream, part2), Timeout);

                hercules.Stream.ReadEvents(stream, 70, end.Payload.Next).ShouldBeEqual(part2);
            }
        }
    }
}