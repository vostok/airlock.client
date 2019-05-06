using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using FluentAssertions.Extensions;
using Vostok.Commons.Testing;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;

namespace Vostok.Hercules.Client.Tests.Functional.Helpers
{
    internal static class HerculesStreamClientExtensions
    {
        public static void WaitForAnyRecord(this IHerculesStreamClient client, string stream)
        {
            var readQuery = new ReadStreamQuery(stream)
            {
                Limit = 1,
                Coordinates = new StreamCoordinates(new StreamPosition[0]),
                ClientShard = 0,
                ClientShardCount = 1
            };

            new Action(() => client.Read(readQuery, 20.Seconds()).Payload.Events.Should().NotBeEmpty())
                .ShouldPassIn(20.Seconds());
        }

        public static List<HerculesEvent> ReadEvents(
            this IHerculesStreamClient client,
            string stream,
            int count,
            int limit = 10000) => client.ReadEvents(stream, count, limit, 1).Single();

        public static List<HerculesEvent>[] ReadEvents(
            this IHerculesStreamClient client,
            string stream,
            int count,
            int limit,
            int clientShards)
        {
            var timeout = 20.Seconds();

            var stopwatch = Stopwatch.StartNew();
            var eventsRead = 0;

            var clientShardTasks = Enumerable.Range(0, clientShards).Select(ReadSingleClientShard);

            var events = Task.WhenAll(clientShardTasks).GetAwaiter().GetResult();
            events.Sum(x => x.Count).Should().Be(count);
            return events;

            async Task<List<HerculesEvent>> ReadSingleClientShard(int clientShard)
            {
                var shardEvents = new List<HerculesEvent>();
                var readQuery = new ReadStreamQuery(stream)
                {
                    Limit = limit,
                    Coordinates = new StreamCoordinates(Array.Empty<StreamPosition>()),
                    ClientShard = clientShard,
                    ClientShardCount = clientShards
                };

                while (stopwatch.Elapsed < timeout && eventsRead < count)
                {
                    var result = await client.ReadAsync(readQuery, timeout);
                    result.IsSuccessful.Should().BeTrue();

                    var eventsFromResponse = result.Payload.Events;

                    shardEvents.AddRange(eventsFromResponse);
                    readQuery.Coordinates = result.Payload.Next;
                    Interlocked.Add(ref eventsRead, eventsFromResponse.Count);
                    await Task.Delay(100);
                }

                return shardEvents;
            }
        }
    }
}