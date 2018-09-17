using System;
using System.Linq;
using System.Net;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;

namespace Vostok.Hercules.Client.Tests
{
    [TestFixture]
    public class RequestMessageBuilderTests
    {
        [Test]
        public void TryAppend_Slice_Success()
        {
            var builder = CreateBuilder(20);

            var buffer = new byte[2];
            var slice = CreateSlice(buffer, 1, 1);

            var actual = builder.TryAppend(slice);

            Assert.True(actual);
        }

        [Test]
        public void TryAppend_Slices_AddsInt32RecordsCount()
        {
            var builder = CreateBuilder(20);

            var buffer1 = new byte[2];
            var slice1 = CreateSlice(buffer1, 1, 1);

            var buffer2 = new byte[2];
            var slice2 = CreateSlice(buffer2, 1, 2);

            builder.TryAppend(slice1);
            builder.TryAppend(slice2);

            Assert.That(BitConverter.ToInt32(builder.Message.Array, 0), Is.EqualTo(IPAddress.HostToNetworkOrder(3)));
        }

        [Test]
        public void TryAppend_Slices_AddsContent()
        {
            var builder = CreateBuilder(20);

            var buffer1 = new byte[] {0, 1};
            var slice1 = CreateSlice(buffer1, 1, 1);

            var buffer2 = new byte[] {0, 2, 3};
            var slice2 = CreateSlice(buffer2, 1, 2);

            builder.TryAppend(slice1);
            builder.TryAppend(slice2);

            builder.Message.Array.Skip(sizeof(int)).Take(3).Should().BeEquivalentTo(new byte[] {1, 2, 3});
        }

        [Test]
        public void TryAppend_BigSliceWhenFirst_Throw()
        {
            var builder = CreateBuilder(20);

            var buffer = new byte[20];
            var slice = CreateSlice(buffer, 1, 1);

            Action actual = () => builder.TryAppend(slice);

            actual.Should().Throw<Exception>();
        }

        [Test]
        public void TryAppend_BigSliceWhenNotFirst_False()
        {
            var builder = CreateBuilder(20);

            var buffer1 = new byte[2];
            var slice1 = CreateSlice(buffer1, 1, 1);

            var buffer2 = new byte[20];
            var slice2 = CreateSlice(buffer2, 1, 1);

            builder.TryAppend(slice1);

            var actual = builder.TryAppend(slice2);

            Assert.False(actual);
        }

        private static RequestMessageBuilder CreateBuilder(int bufferLength) =>
            new RequestMessageBuilder(new byte[bufferLength]);

        private static BufferSlice CreateSlice(byte[] buffer, int offset, int recordsCount) =>
            new BufferSlice(Substitute.For<IBuffer>(), buffer, offset, buffer.Length - offset, recordsCount);
    }
}