using System;
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
using Vostok.Logging.Console;

namespace Vostok.Hercules.Client.Tests.Integration
{
    internal class HerculesSink_IntegrationTests
    {
        private const int GuidSize = 16;
        private const int EventIdOffset = sizeof(byte) + sizeof(long);

        private ITransport transport;
        private HerculesSink sink;
        private Request lastRequest;

        [SetUp]
        public void Setup()
        {
            transport = Substitute.For<ITransport>();
            transport.Capabilities.Returns(
                TransportCapabilities.RequestStreaming |
                TransportCapabilities.ResponseStreaming |
                TransportCapabilities.RequestCompositeBody);

            var settings = new HerculesSinkSettings(new FixedClusterProvider(new Uri("http://localhost/")), () => "apiKey")
            {
                AdditionalSetup = c => c.Transport = transport,
                SendPeriod = 100.Milliseconds()
            };

            sink = new HerculesSink(settings, new SynchronousConsoleLog());

            SetResponse(Responses.Ok);
        }

        [Test]
        public void Should_send_correct_binary_data_to_gateway()
        {
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

            const byte EventProtocolVersion = 1;
            const byte IntegerTag = 4;
            const byte LongTag = 5;

            var writer = new BinaryBufferWriter(size) {Endianness = Endianness.Big};
            writer.Write(EventProtocolVersion);
            writer.Write(unixTimestamp);
            writer.Write(Guid.Empty);

            writer.Write((ushort)2);

            // first tag
            writer.Write((byte)byteKey1.Length);
            writer.WriteWithoutLength(byteKey1);
            writer.Write(IntegerTag);
            writer.Write(value1);

            // second tag
            writer.Write((byte)byteKey2.Length);
            writer.WriteWithoutLength(byteKey2);
            writer.Write(LongTag);
            writer.Write(value2);

            var recordCountContent = new byte[] {0, 0, 0, 1};
            var recordContent = writer.FilledSegment;

            sink.Put(
                "stream",
                builder => builder
                    .SetTimestamp(timestamp)
                    .AddValue(key1, value1)
                    .AddValue(key2, value2));

            new Action(
                () =>
                {
                    var actualRecordsCountContent = lastRequest?.CompositeContent?.Parts[0]?.ToArraySegment();
                    var actualRecordContent = lastRequest?.CompositeContent?.Parts[1]?.ToArray();
                    actualRecordContent.Should().NotBeNull();
                    EraseEventId(actualRecordContent);

                    actualRecordsCountContent.Should().BeEquivalentTo(recordCountContent, c => c.WithStrictOrdering());
                    actualRecordContent.Should().BeEquivalentTo(recordContent, c => c.WithStrictOrdering());
                }).ShouldPassIn(5.Seconds());
        }

        private static void EraseEventId(byte[] data) =>
            Fill(data, 0, EventIdOffset, GuidSize);

        private static void Fill(byte[] arr, byte value, int startIndex, int count)
        {
            for (var i = 0; i < count; i++)
                arr[startIndex + i] = value;
        }

        private void SetResponse(Response response) =>
            transport
                .SendAsync(null, TimeSpan.Zero, TimeSpan.Zero, CancellationToken.None)
                .ReturnsForAnyArgs(response)
                .AndDoes(x => lastRequest = x.Arg<Request>());
    }
}