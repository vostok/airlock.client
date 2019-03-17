using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Binary;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Requests;

namespace Vostok.Hercules.Client.Tests.Sink.Requests
{
    [TestFixture]
    internal class RequestContentFactory_Tests
    {
        private RequestContentFactory factory;

        [SetUp]
        public void TestSetup()
        {
            factory = new RequestContentFactory();
        }

        [Test]
        public void Should_produce_a_well_formed_request_body_from_several_snapshots()
        {
            var data1 = Guid.NewGuid().ToByteArray();
            var data2 = Guid.NewGuid().ToByteArray();
            var data3 = Guid.NewGuid().ToByteArray();

            var snapshot1 = Snapshot(data1, 2);
            var snapshot2 = Snapshot(data2, 5);
            var snapshot3 = Snapshot(data3, 1);

            var body = factory.CreateContent(new[] {snapshot1, snapshot2, snapshot3}, out var recordsCount, out var recordsSize);

            recordsCount.Should().Be(8);
            recordsSize.Should().Be(48);

            body.Parts.Should().HaveCount(4);

            new BinaryBufferReader(body.Parts[0].Buffer, 0) {Endianness = Endianness.Big}.ReadInt32().Should().Be(recordsCount);

            body.Parts[1].ToArray().Should().Equal(data1);
            body.Parts[2].ToArray().Should().Equal(data2);
            body.Parts[3].ToArray().Should().Equal(data3);
        }

        private static BufferSnapshot Snapshot(byte[] data, int records)
        {
            var source = Substitute.For<IBuffer>();
            var garbage = Guid.NewGuid().ToByteArray();
            var buffer = data.Concat(garbage).ToArray();

            return new BufferSnapshot(source, new BufferState(data.Length, records), buffer);
        }
    }
}