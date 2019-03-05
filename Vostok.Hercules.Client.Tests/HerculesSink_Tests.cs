using System;
using System.Linq;
using System.Text;
using System.Threading;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Clusterclient.Core.Transport;
using Vostok.Commons.Binary;
using Vostok.Commons.Testing;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Hercules.Client.Tests
{
    internal class HerculesSink_Tests
    {
        private string apiKey;
        private ITransport transport;
        private HerculesSinkSettings config;
        private HerculesSink client;
        private Request lastRequest;

        [SetUp]
        public void Setup()
        {
            client?.Dispose();

            lastRequest = null;

            apiKey = Guid.NewGuid().ToString();

            transport = Substitute.For<ITransport>();

            transport.Capabilities.Returns(TransportCapabilities.RequestStreaming | TransportCapabilities.ResponseStreaming | TransportCapabilities.RequestCompositeBody);

            config = new HerculesSinkSettings(new FixedClusterProvider(new Uri("http://example.com/dev/null")), () => apiKey)
            {
                ClusterClientSetup = c => c.Transport = transport
            };

            client = new HerculesSink(config, new SynchronousConsoleLog());

            SetResponse(Responses.Ok);
        }

        [Test]
        public void LostRecordsCount_should_not_grows_infinitely_when_gate_is_offline()
        {
            SetResponse(Responses.BadRequest);

            client.Put("stream", x => x.AddValue("key", true));

            var action = new Action(() => client.LostRecordsCount.Should().Be(1));
            action.ShouldPassIn(5.Seconds());
            action.ShouldNotFailIn(5.Seconds());
        }

        [TestCase(1, TestName = "Should_pass_global_api_key_to_hercules_sink")]
        [TestCase(3, TestName = "Should_not_cache_global_api_key")]
        public void ApiKeyTests(int iterations)
        {
            for (var i = 0; i < iterations; ++i)
            {
                client.Put("stream", _ => {});
                new Action(
                    () =>
                    {
                        var apiKeyHeaderValue = lastRequest?.Headers?["apiKey"];
                        apiKeyHeaderValue.Should().Be(apiKey);
                    }).ShouldPassIn(5.Seconds());
                apiKey = Guid.NewGuid().ToString();
            }
        }

        [TestCase(1, TestName = "Should_support_per_stream_api_key_overriding")]
        [TestCase(3, TestName = "Should_not_cache_per_stream_api_key")]
        public void PerStreamApiKeyTests(int iterations)
        {
            var perStreamApiKey = Guid.NewGuid().ToString();

            client.ConfigureStream(
                "stream",
                new StreamSettings
                {
                    ApiKeyProvider = () => perStreamApiKey
                });

            for (var i = 0; i < iterations; ++i)
            {
                client.Put("stream", _ => {});
                new Action(
                    () =>
                    {
                        var apiKeyHeaderValue = lastRequest?.Headers?["apiKey"];
                        apiKeyHeaderValue.Should().Be(perStreamApiKey);
                    }).ShouldPassIn(5.Seconds());
                perStreamApiKey = Guid.NewGuid().ToString();
            }
        }

        [Test]
        public void Should_pass_stream_name_header()
        {
            var streamName = Guid.NewGuid().ToString();
            client.Put(streamName, _ => {});
            new Action(
                () =>
                {
                    var query = lastRequest?.Url.Query;
                    query.Should().Be($"?{Constants.StreamQueryParameter}={streamName}");
                }).ShouldPassIn(5.Seconds());
        }

        [Test]
        public void Should_send_record()
        {
            const int GuidSize = 16;

            var key1 = "intValue";
            var key2 = "longValue";
            var byteKey1 = Encoding.UTF8.GetBytes(key1);
            var byteKey2 = Encoding.UTF8.GetBytes(key2);
            var value1 = 100500;
            var value2 = (long)1e16 + 5;
            var timestamp = DateTime.UtcNow;
            var unixTimestamp = EpochHelper.ToUnixTimeUtcTicks(timestamp);

            var size =
                +sizeof(byte) // protocol version
                + sizeof(long) // timestamp
                + GuidSize // guid
                + sizeof(ushort) // tag count
                + sizeof(byte) // key length
                + byteKey1.Length // key
                + sizeof(byte) // type tag
                + sizeof(int) // value
                + sizeof(byte) // key length
                + byteKey2.Length // key
                + sizeof(byte) // type tag
                + sizeof(long); // value

            var uuidOffset =
                +sizeof(byte) // protocol version
                + sizeof(long); // timestamp

            var writer = new BinaryBufferWriter(size) {Endianness = Endianness.Big};
            writer.Write(Constants.ProtocolVersion);
            writer.Write(unixTimestamp);
            writer.Write(Guid.Empty);

            writer.Write((ushort)2);

            // first tag
            writer.Write((byte)byteKey1.Length);
            writer.WriteWithoutLength(byteKey1);
            writer.Write((byte)TagType.Integer);
            writer.Write(value1);

            // second tag
            writer.Write((byte)byteKey2.Length);
            writer.WriteWithoutLength(byteKey2);
            writer.Write((byte)TagType.Long);
            writer.Write(value2);

            var recordCountContent = new byte[] {0, 0, 0, 1};
            var recordContent = writer.FilledSegment;

            client.Put(
                "stream",
                builder => builder
                    .SetTimestamp(timestamp)
                    .AddValue(key1, value1)
                    .AddValue(key2, value2));
            new Action(
                () =>
                {
                    var first = lastRequest?.CompositeContent?.Parts[0]?.ToArraySegment();
                    var second = lastRequest?.CompositeContent?.Parts[1]?.ToArray();
                    second.Should().NotBeNull();
                    Fill(second, 0, uuidOffset, GuidSize);

                    Console.WriteLine(string.Join(" ", second.Select(x => x.ToString("x2"))));

                    first.Should().BeEquivalentTo(recordCountContent, c => c.WithStrictOrdering());
                    second.Should().BeEquivalentTo(recordContent, c => c.WithStrictOrdering());
                }).ShouldPassIn(5.Seconds());
        }

        [Test]
        [Explicit]
        public void Test()
        {
            var config = new HerculesSinkSettings(new FixedClusterProvider(new Uri("")), () => "");

            var client = new HerculesSink(config, new ConsoleLog());

            client.Put("", x => { x.AddValue("key", true); });

            Thread.Sleep(1000000);
        }

        private void SetResponse(Response response) =>
            transport
                .SendAsync(null, TimeSpan.Zero, TimeSpan.Zero, CancellationToken.None)
                .ReturnsForAnyArgs(response)
                .AndDoes(x => lastRequest = x.Arg<Request>());

        private static void Fill(byte[] arr, byte value, int startIndex, int count)
        {
            for (var i = 0; i < count; i++)
                arr[startIndex + i] = value;
        }
    }
}