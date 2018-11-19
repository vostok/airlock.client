using System;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Vostok.Hercules.Client.Tests
{
    [TestFixture]
    public class BufferSliceFactoryTests
    {
        [Test]
        public void Cut_EmptySnapshot_ReturnsEmpty()
        {
            var parent = CreateBuffer();
            var buffer = CreateUnderlyingBuffer();

            var snapshot = new BufferSnapshot(parent, buffer, 0, 0);

            var instance = CreateInstance(1);

            var actual = instance.Cut(snapshot).ToArray();

            actual.Should().BeEmpty();
        }

        [Test]
        //TODO: what is this test do?
        public void Cut_MaxSliceSizeNotExceeded_ByAllRecords_ReturnsOne()
        {
            var parent = CreateBuffer();
            var buffer = CreateUnderlyingBuffer();

            parent.GetRecordSize(0).ReturnsForAnyArgs(1);

            var snapshot = new BufferSnapshot(parent, buffer, 2, 2);

            var instance = CreateInstance(2);

            var actual = instance.Cut(snapshot).ToArray();

            actual.Should().HaveCount(1);
            actual[0].Should().BeEquivalentTo(new BufferSlice(parent, buffer, 0, 2, 2));
        }

        [Test]
        public void Cut_MaxSliceSizeExceeded_ByOneRecord_Throw()
        {
            var parent = CreateBuffer();
            var buffer = CreateUnderlyingBuffer();

            parent.GetRecordSize(0).Returns(2);
            var snapshot = new BufferSnapshot(parent, buffer, 2, 1);

            var instance = CreateInstance(1);

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Action actual = () => instance.Cut(snapshot).ToArray();

            actual.Should().Throw<InvalidOperationException>();
        }

        [Test]
        public void Cut_ManyRecords_Returns()
        {
            var parent = CreateBuffer();
            var buffer = CreateUnderlyingBuffer();

            parent.GetRecordSize(0).Returns(1);
            parent.GetRecordSize(1).Returns(2);
            parent.GetRecordSize(3).Returns(3);
            var snapshot = new BufferSnapshot(parent, buffer, 6, 3);

            var instance = CreateInstance(4);

            var actual = instance.Cut(snapshot).ToArray();

            actual.Should().HaveCount(2);
            actual[0].Should().BeEquivalentTo(new BufferSlice(parent, buffer, 0, 3, 2));
            actual[1].Should().BeEquivalentTo(new BufferSlice(parent, buffer, 3, 3, 1));
        }

        private static IBuffer CreateBuffer() => Substitute.For<IBuffer>();

        private static byte[] CreateUnderlyingBuffer() => Array.Empty<byte>();

        private static BufferSliceFactory CreateInstance(int maxSliceSize) => new BufferSliceFactory(maxSliceSize);
    }
}