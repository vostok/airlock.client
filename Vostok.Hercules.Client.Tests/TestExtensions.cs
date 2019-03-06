using System;
using FluentAssertions;
using Vostok.Commons.Testing;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;

namespace Vostok.Hercules.Client.Tests
{
    internal static class TestExtensions
    {
        public static void CreateStreamAndWait(this IHerculesManagementClient client, CreateStreamQuery query)
        {
            client.CreateStream(query, 10.Seconds()).EnsureSuccess();
            new Action(
                    () => client.ListStreams(10.Seconds())
                        .Payload
                        .Should()
                        .Contain(query.Name))
                .ShouldPassIn(20.Seconds());
        }

        public static void WaitForAnyRecord(this IHerculesStreamClient client, string stream)
        {
            var readQuery = new ReadStreamQuery(stream)
            {
                Limit = 100,
                Coordinates = new StreamCoordinates(new StreamPosition[0]),
                ClientShard = 0,
                ClientShardCount = 1
            };

            new Action(() => client.Read(readQuery, 20.Seconds()).Payload.Events.Should().NotBeEmpty())
                .ShouldPassIn(20.Seconds());
        }
    }
}