using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Hercules.Client.Tests.Functional.Helpers;

namespace Vostok.Hercules.Client.Tests.Functional
{
    [TestFixture]
    internal class HerculesSink_FunctionalTests : HerculesSender_FunctionalTests
    {
        public HerculesSink_FunctionalTests() =>
            PushEvent = (stream, e) => Hercules.Sink.Put(stream, e);

        [TestCase(10000, 1)]
        [TestCase(10000, 10)]
        [TestCase(50000, 2)]
        public void Should_write_many_events(int count, int threads)
        {
            var builders = TestHelpers.GenerateEventBuilders(count);

            using (Hercules.Management.CreateTemporaryStream(out var stream))
            {
                builders.PushEvents(PushEvent.ToStream(stream), threads);

                var events = Hercules.Stream.ReadEvents(stream, count, count / 4);

                events.ShouldBeEqual(builders.ToEvents());
            }
        }

        [TestCase(50000)]
        public void Should_read_events_in_many_client_shards(int count)
        {
            var builders = TestHelpers.GenerateEventBuilders(count);

            using (Hercules.Management.CreateTemporaryStream(out var stream))
            {
                builders.PushEvents(PushEvent.ToStream(stream));

                var shards = Hercules.Stream.ReadEvents(stream, count, count / 4, 3);

                shards.SelectMany(x => x).ShouldBeEqual(builders.ToEvents());

                foreach (var shard in shards)
                    shard.Should().NotBeEmpty();
            }
        }
    }
}