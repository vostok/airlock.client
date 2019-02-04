using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Commons.Testing;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Management;
using Vostok.Logging.Console;

namespace Vostok.Hercules.Client.Tests
{
    public class HerculesSinkFunctionalTests
    {
        private readonly TimeSpan timeout = 20.Seconds();
        private readonly ConsoleLog log = new ConsoleLog();

        private string stream;
        private string gateUrl = "http://vm-hercules04:6306";
        private string apiKey = "dotnet_api_key";
        private string managementApiUrl = "http://vm-hercules05:6507";
        private string streamApiUrl = "http://vm-hercules05:6407";
        private TimeSpan ttl = 20.Seconds();
        private HerculesStreamClient streamClient;
        private HerculesSink sink;
        private HerculesManagementClient managementClient;
        private HerculesStreamClientConfig streamClientConfig;

        [SetUp]
        public void Setup()
        {
            stream = $"dotnet_test_{Guid.NewGuid().ToString().Substring(0, 8)}";
            // var dt = DateTime.UtcNow;
            // var key = dt.Hour * 3600 + dt.Minute * 60 + dt.Second;
            // stream = $"dotnet_test_{key}";

            var sinkConfig = new HerculesSinkConfig(new FixedClusterProvider(new Uri(gateUrl)), () => apiKey);

            managementClient = new HerculesManagementClient(
                new HerculesManagementClientConfig
                {
                    Cluster = new FixedClusterProvider(new Uri(managementApiUrl)),
                    ServiceName = "HerculesManagementApi",
                    ApiKeyProvider = () => apiKey
                },
                log);

            sink = new HerculesSink(sinkConfig, log);

            streamClientConfig = new HerculesStreamClientConfig(
                new FixedClusterProvider(new Uri(streamApiUrl)),
                () => apiKey);

            streamClient = new HerculesStreamClient(streamClientConfig, log);

            managementClient.CreateStream(
                    new CreateStreamQuery(
                        new StreamDescription(stream)
                        {
                            TTL = ttl,
                            Partitions = 3
                        }),
                    timeout)
                .EnsureSuccess();
        }

        [TearDown]
        public void TearDown()
        {
            managementClient.DeleteStream(stream, timeout).EnsureSuccess();
        }

        [Test, Explicit]
        public void Should_read_and_write_one_hercules_event()
        {
            var intValue = 100500;
            var intArray = new[] {1, 2, 3};

            sink.Put(
                stream,
                x => x
                    .AddValue("key", intValue)
                    .AddVector("vec", intArray)
                    .AddContainer("container", builder => builder.AddValue("x", 5))
                    .AddNull("nullField"));

            var readQuery = new ReadStreamQuery(stream)
            {
                Limit = 100,
                Coordinates = new StreamCoordinates(new StreamPosition[0]),
                ClientShard = 0,
                ClientShardCount = 1
            };

            new Action(() => streamClient.Read(readQuery, timeout).Payload.Events.Should().NotBeEmpty())
                .ShouldPassIn(timeout);

            sink.SentRecordsCount.Should().Be(1);

            var readStreamResult = streamClient.Read(readQuery, timeout);

            readStreamResult.Status.Should().Be(HerculesStatus.Success);
            readStreamResult.Payload.Events.Should().HaveCount(1);

            var @event = readStreamResult.Payload.Events[0];

            @event.Tags["key"].AsInt.Should().Be(intValue);
            @event.Tags["vec"]
                .AsVector.AsIntList.Should()
                .BeEquivalentTo(
                    intArray,
                    o => o.WithStrictOrdering());
            @event.Tags["container"].AsContainer["x"].AsInt.Should().Be(5);
            @event.Tags["nullField"].IsNull.Should().BeTrue();
        }

        [Test, Explicit]
        public void Should_read_and_write_hercules_events()
        {
            var count = 100_000;

            var seen = new bool[count];

            for (var i = 0; i < count; ++i)
            {
                var t = i;
                sink.Put(stream, x => x.AddValue("key", t));
            }

            var read = 0;
            var state = new StreamCoordinates(new StreamPosition[0]);

            while (read < count)
            {
                var readQuery = new ReadStreamQuery(stream)
                {
                    Limit = 10000,
                    Coordinates = state,
                    ClientShard = 0,
                    ClientShardCount = 1
                };

                var readStreamResult = streamClient.Read(readQuery, timeout);

                readStreamResult.Status.Should().Be(HerculesStatus.Success);

                foreach (var @event in readStreamResult.Payload.Events)
                {
                    seen[@event.Tags["key"].AsInt] = true;
                    read++;
                }
                
                sink.LostRecordsCount.Should().Be(0);

                state = readStreamResult.Payload.Next;
            }
            
            seen.Should().AllBeEquivalentTo(true);
        }

        [Test, Explicit]
        public void Should_read_and_write_hercules_event_with_vector_of_containers()
        {
            sink.Put(
                stream,
                x => x
                    .AddVectorOfContainers(
                        "tag",
                        new List<Action<IHerculesTagsBuilder>>
                        {
                            b => b.AddValue("a", 0),
                            b => b.AddValue("a", 1),
                            b => b.AddValue("a", 2),
                        }));

            var readQuery = new ReadStreamQuery(stream)
            {
                Limit = 100,
                Coordinates = new StreamCoordinates(new StreamPosition[0]),
                ClientShard = 0,
                ClientShardCount = 1
            };

            new Action(() => streamClient.Read(readQuery, timeout).Payload.Events.Should().NotBeEmpty())
                .ShouldPassIn(timeout);

            sink.SentRecordsCount.Should().Be(1);

            var readStreamResult = streamClient.Read(readQuery, timeout);

            readStreamResult.Status.Should().Be(HerculesStatus.Success);
            readStreamResult.Payload.Events.Should().HaveCount(1);

            var @event = readStreamResult.Payload.Events[0];

            @event.Tags["tag"].AsVector.AsContainerList[0]["a"].AsInt.Should().Be(0);
            @event.Tags["tag"].AsVector.AsContainerList[1]["a"].AsInt.Should().Be(1);
            @event.Tags["tag"].AsVector.AsContainerList[2]["a"].AsInt.Should().Be(2);
        }

        [Test, Explicit]
        public void Should_read_and_write_hercules_event_with_string_values()
        {
            sink.Put(
                stream,
                x => x
                    .AddValue("k1", "v1")
                    .AddValue("k2", "v2"));

            var readQuery = new ReadStreamQuery(stream)
            {
                Limit = 100,
                Coordinates = new StreamCoordinates(new StreamPosition[0]),
                ClientShard = 0,
                ClientShardCount = 1
            };

            new Action(() => streamClient.Read(readQuery, timeout).Payload.Events.Should().NotBeEmpty())
                .ShouldPassIn(timeout);

            sink.SentRecordsCount.Should().Be(1);

            var readStreamResult = streamClient.Read(readQuery, timeout);

            readStreamResult.Status.Should().Be(HerculesStatus.Success);
            readStreamResult.Payload.Events.Should().HaveCount(1);

            var @event = readStreamResult.Payload.Events[0];

            @event.Tags["k1"].AsString.Should().Be("v1");
            @event.Tags["k2"].AsString.Should().Be("v2");
        }

        [Test, Explicit]
        public void Should_delete_stream()
        {
            sink.Put(stream, x => x.AddValue("key", 1));

            var state = new StreamCoordinates(new StreamPosition[0]);
            
            var readQuery = new ReadStreamQuery(stream)
            {
                Limit = 10000,
                Coordinates = state,
                ClientShard = 0,
                ClientShardCount = 1
            };
            
            new Action(() => streamClient.Read(readQuery, timeout).Payload.Events.Count.Should().BePositive())
                .ShouldPassIn(timeout);

            managementClient.DeleteStream(stream, timeout);

            streamClient.Read(readQuery, timeout).IsSuccessful.Should().BeFalse();

            managementClient.CreateStream(new CreateStreamQuery(new StreamDescription(stream)
            {
                Partitions = 3,
                TTL = 1.Minutes()
            }), timeout);

            streamClient.Read(readQuery, timeout).Payload.Events.Count.Should().Be(0);
        }
    }
}