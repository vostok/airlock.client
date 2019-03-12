using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Clusterclient.Topology.CC;
using Vostok.ClusterConfig.Client;
using Vostok.Commons.Testing;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Management;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Hercules.Client.Tests
{
    internal class HerculesSink_FunctionalTests
    {
        private readonly TimeSpan timeout = 20.Seconds();
        private readonly ConsoleLog log = new ConsoleLog();
        private readonly ClusterConfigClient clusterConfigClient = new ClusterConfigClient();
        private readonly string gateTopology = "topology/hercules/gate.test";
        private readonly string apiKey = "dotnet_api_key";
        private readonly string managementApiTopology = "topology/hercules/management-api.test";
        private readonly string streamApiTopology = "topology/hercules/stream-api.test";
        private readonly TimeSpan ttl = 20.Seconds();

        private string stream;
        private HerculesStreamClient streamClient;
        private HerculesSink sink;
        private HerculesManagementClient managementClient;
        private HerculesStreamClientSettings streamClientSettings;

        [SetUp]
        public void Setup()
        {
            stream = $"dotnet_test_csharpclient_{Guid.NewGuid().ToString().Substring(0, 8)}";

            var sinkConfig = new HerculesSinkSettings(
                new ClusterConfigClusterProvider(clusterConfigClient, gateTopology, log),
                () => apiKey);

            managementClient = new HerculesManagementClient(
                new HerculesManagementClientSettings(
                    new ClusterConfigClusterProvider(clusterConfigClient, managementApiTopology, log),
                    () => apiKey
                ),
                log);

            sink = new HerculesSink(sinkConfig, log);

            streamClientSettings = new HerculesStreamClientSettings(
                new ClusterConfigClusterProvider(clusterConfigClient, streamApiTopology, log),
                () => apiKey);

            streamClient = new HerculesStreamClient(streamClientSettings, log);

            managementClient.CreateStreamAndWait(
                new CreateStreamQuery(stream)
                {
                    TTL = ttl,
                    Partitions = 3
                });
        }

        [TearDown]
        public void TearDown()
        {
            managementClient.DeleteStream(stream, timeout).EnsureSuccess();
        }

        [Test]
        [Explicit]
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
                Limit = 10000,
                Coordinates = new StreamCoordinates(new StreamPosition[0]),
                ClientShard = 0,
                ClientShardCount = 1
            };

            streamClient.WaitForAnyRecord(stream);

            sink.GetStatistics().Global.SentRecords.Count.Should().Be(1);

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

        [Test]
        [Explicit]
        public void Should_not_fail_on_duplicate_keys()
        {
            sink.Put(
                stream,
                x => x
                    .AddValue("key", 1)
                    .AddValue("key", 2));

            var readQuery = new ReadStreamQuery(stream)
            {
                Limit = 10000,
                Coordinates = new StreamCoordinates(new StreamPosition[0]),
                ClientShard = 0,
                ClientShardCount = 1
            };

            streamClient.WaitForAnyRecord(stream);

            sink.GetStatistics().Global.SentRecords.Count.Should().Be(1);

            var readStreamResult = streamClient.Read(readQuery, timeout);

            readStreamResult.Status.Should().Be(HerculesStatus.Success);
            readStreamResult.Payload.Events.Should().HaveCount(1);

            var @event = readStreamResult.Payload.Events[0];

            @event.Tags["key"].AsInt.Should().Be(2);
        }

        [Explicit]
        [TestCase(1, 100_000)]
        [TestCase(2, 100_000)]
        [TestCase(2, 250_000)]
        [TestCase(10, 50_000)]
        [TestCase(50, 10_000)]
        public void Should_read_and_write_hercules_events(int writers, int countPerWriter)
        {
            var seen = new bool[writers][];

            for (var i = 0; i < writers; i++)
                seen[i] = new bool[countPerWriter];

            for (var t = 0; t < writers; ++t)
            {
                var writer = t;
                Task.Run(
                    () =>
                    {
                        for (var i = 0; i < countPerWriter; ++i)
                            sink.Put(stream, x => x.AddValue("writer", writer).AddValue("record", i));
                    });
            }

            var read = 0;
            var state = new StreamCoordinates(new StreamPosition[0]);

            while (read < countPerWriter * writers)
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

                new Action(() => sink.GetStatistics().Global.SentRecords.Count.Should().Be(writers * countPerWriter)).ShouldPassIn(1.Minutes());

                foreach (var @event in readStreamResult.Payload.Events)
                {
                    seen[@event.Tags["writer"].AsInt][@event.Tags["record"].AsInt] = true;
                    read++;
                }

                state = readStreamResult.Payload.Next;
            }

            seen.SelectMany(x => x).Should().AllBeEquivalentTo(true);
        }

        [Test]
        [Explicit]
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
                            b => b.AddValue("a", 2)
                        }));

            var readQuery = new ReadStreamQuery(stream)
            {
                Limit = 100,
                Coordinates = new StreamCoordinates(new StreamPosition[0]),
                ClientShard = 0,
                ClientShardCount = 1
            };

            streamClient.WaitForAnyRecord(stream);

            sink.GetStatistics().Global.SentRecords.Count.Should().Be(1);

            var readStreamResult = streamClient.Read(readQuery, timeout);

            readStreamResult.Status.Should().Be(HerculesStatus.Success);
            readStreamResult.Payload.Events.Should().HaveCount(1);

            var @event = readStreamResult.Payload.Events[0];

            @event.Tags["tag"].AsVector.AsContainerList[0]["a"].AsInt.Should().Be(0);
            @event.Tags["tag"].AsVector.AsContainerList[1]["a"].AsInt.Should().Be(1);
            @event.Tags["tag"].AsVector.AsContainerList[2]["a"].AsInt.Should().Be(2);
        }

        [Test]
        [Explicit]
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

            streamClient.WaitForAnyRecord(stream);

            sink.GetStatistics().Global.SentRecords.Count.Should().Be(1);

            var readStreamResult = streamClient.Read(readQuery, timeout);

            readStreamResult.Status.Should().Be(HerculesStatus.Success);
            readStreamResult.Payload.Events.Should().HaveCount(1);

            var @event = readStreamResult.Payload.Events[0];

            @event.Tags["k1"].AsString.Should().Be("v1");
            @event.Tags["k2"].AsString.Should().Be("v2");
        }

        [Test]
        [Explicit]
        public void Should_read_and_write_hercules_event_with_all_data_types()
        {
            var guid = Guid.NewGuid();
            var @bool = true;
            var @byte = (byte)42;
            var @double = Math.PI;
            var @float = (float)@double;
            var @int = int.MaxValue;
            var @long = long.MinValue;
            var @short = short.MinValue;
            var @string = "dotnet";

            var guidVec = new[] {Guid.NewGuid(), Guid.NewGuid()};
            var boolVec = new[] {true, false};
            var byteVec = new[] {(byte)42, (byte)25};
            var doubleVec = new[] {Math.PI, Math.E};
            var floatVec = doubleVec.Select(x => (float)x).ToArray();
            var intVec = new[] {1337, 31337, int.MaxValue, int.MinValue};
            var longVec = new[] {long.MaxValue, long.MinValue, (long)1e18 + 1};
            var shortVec = new short[] {1000, 2000};
            var stringVec = new[] {"dotnet", "hercules"};

            sink.Put(
                stream,
                x => x
                    .AddNull("null")
                    .AddValue("guid", guid)
                    .AddValue("bool", @bool)
                    .AddValue("byte", @byte)
                    .AddValue("double", @double)
                    .AddValue("float", @float)
                    .AddValue("int", @int)
                    .AddValue("long", @long)
                    .AddValue("short", @short)
                    .AddValue("string", @string)
                    // Bug with Guid vector on server side: https://yt.skbkontur.ru/issue/HERCULES-226
                    // .AddVector("guidVec", guidVec)
                    .AddVector("boolVec", boolVec)
                    .AddVector("byteVec", byteVec)
                    .AddVector("doubleVec", doubleVec)
                    .AddVector("floatVec", floatVec)
                    .AddVector("intVec", intVec)
                    .AddVector("longVec", longVec)
                    .AddVector("shortVec", shortVec)
                    .AddVector("stringVec", stringVec)
                    .AddVector("emptyVec", new int[0])
                    .AddContainer(
                        "container",
                        b => b
                            .AddValue("inner", "x")
                            .AddVector("innerVec", new[] {1, 2, 3}))
                    .AddVectorOfContainers(
                        "containerVec",
                        new Action<IHerculesTagsBuilder>[]
                        {
                            b => b
                                .AddValue("inner", "y")
                                .AddVector("innerVec", new long[] {1, 3, 5})
                        })
                    .AddVectorOfContainers("emptyContainerVec", new Action<IHerculesTagsBuilder>[0])
            );

            void Assert(HerculesTags tags)
            {
                tags["null"].IsNull.Should().BeTrue();

                tags["guid"].AsGuid.Should().Be(guid);
                tags["bool"].AsBool.Should().Be(@bool);
                tags["byte"].AsByte.Should().Be(@byte);
                tags["double"].AsDouble.Should().Be(@double);
                tags["float"].AsFloat.Should().Be(@float);
                tags["int"].AsInt.Should().Be(@int);
                tags["long"].AsLong.Should().Be(@long);
                tags["short"].AsShort.Should().Be(@short);
                tags["string"].AsString.Should().Be(@string);
                // tags["guidVec"].AsVector.AsGuidList.Should().BeEquivalentTo(guidVec, c => c.WithStrictOrdering());
                tags["boolVec"].AsVector.AsBoolList.Should().BeEquivalentTo(boolVec, c => c.WithStrictOrdering());
                tags["byteVec"].AsVector.AsByteList.Should().BeEquivalentTo(byteVec, c => c.WithStrictOrdering());
                tags["doubleVec"].AsVector.AsDoubleList.Should().BeEquivalentTo(doubleVec, c => c.WithStrictOrdering());
                tags["floatVec"].AsVector.AsFloatList.Should().BeEquivalentTo(floatVec, c => c.WithStrictOrdering());
                tags["intVec"].AsVector.AsIntList.Should().BeEquivalentTo(intVec, c => c.WithStrictOrdering());
                tags["longVec"].AsVector.AsLongList.Should().BeEquivalentTo(longVec, c => c.WithStrictOrdering());
                tags["shortVec"].AsVector.AsShortList.Should().BeEquivalentTo(shortVec, c => c.WithStrictOrdering());
                tags["stringVec"].AsVector.AsStringList.Should().BeEquivalentTo(stringVec, c => c.WithStrictOrdering());
                tags["emptyVec"].AsVector.AsIntList.Should().BeEmpty();

                tags["container"].AsContainer["inner"].AsString.Should().Be("x");
                tags["container"].AsContainer["innerVec"].AsVector.AsIntList.Should().BeEquivalentTo(new[] {1, 2, 3}, c => c.WithStrictOrdering());

                tags["containerVec"].AsVector.AsContainerList[0]["inner"].AsString.Should().Be("y");
                tags["containerVec"].AsVector.AsContainerList[0]["innerVec"].AsVector.AsLongList.Should().BeEquivalentTo(new long[] {1, 3, 5}, c => c.WithStrictOrdering());

                tags["emptyContainerVec"].AsVector.AsContainerList.Should().BeEmpty();
            }

            var readQuery = new ReadStreamQuery(stream)
            {
                Limit = 100,
                Coordinates = new StreamCoordinates(new StreamPosition[0]),
                ClientShard = 0,
                ClientShardCount = 1
            };

            streamClient.WaitForAnyRecord(stream);

            sink.GetStatistics().Global.SentRecords.Count.Should().Be(1);

            var readStreamResult = streamClient.Read(readQuery, timeout);

            readStreamResult.Status.Should().Be(HerculesStatus.Success);
            readStreamResult.Payload.Events.Should().HaveCount(1);

            var @event = readStreamResult.Payload.Events[0];

            Assert(@event.Tags);
        }

        [Test]
        [Explicit]
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

            streamClient.WaitForAnyRecord(stream);

            managementClient.DeleteStream(stream, timeout);

            streamClient.Read(readQuery, timeout).IsSuccessful.Should().BeFalse();

            managementClient.CreateStream(
                new CreateStreamQuery(stream)
                {
                    Partitions = 3,
                    TTL = 1.Minutes()
                },
                timeout);

            streamClient.Read(readQuery, timeout).Payload.Events.Count.Should().Be(0);
        }

        [Test]
        [Explicit]
        public void Should_not_fall_into_infinite_loop_after_creation()
        {
            new HerculesSink(new HerculesSinkSettings(new FixedClusterProvider(new Uri("http://localhost/")), () => ""), new SilentLog())
                .GetHashCode();
            Thread.Sleep(20.Seconds());
            // see for cpu usage
        }

        [Test]
        public void t()
        {
            var herculesSink = new HerculesSink(new HerculesSinkSettings(
                new FixedClusterProvider(new Uri("http://localhost:10101")), () => "api"), new SilentLog());

            var sw = Stopwatch.StartNew();
            while (sw.Elapsed < TimeSpan.FromSeconds(1))
                herculesSink.Put("some", builder => builder.AddValue("a", "ololololololo"));

            
            Thread.Sleep(5000);
            var stats = herculesSink.GetStatistics().Global;
            Console.WriteLine(stats.SentRecords);
            Console.WriteLine(stats.LostRecords);
        }
    }
    
    internal class tr : ITransport
    {
        public async Task<Response> SendAsync(Request request, TimeSpan? connectionTimeout, TimeSpan timeout, CancellationToken cancellationToken)
        {
            await Task.Delay(1, cancellationToken);
            return Responses.Ok;
        }

        public TransportCapabilities Capabilities { get; } = TransportCapabilities.RequestStreaming | TransportCapabilities.ResponseStreaming | TransportCapabilities.RequestCompositeBody;
    }
}