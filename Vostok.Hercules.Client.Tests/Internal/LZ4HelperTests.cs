using FluentAssertions;
using K4os.Compression.LZ4;
using NUnit.Framework;
using Vostok.Hercules.Client.Internal;
// ReSharper disable InconsistentNaming

namespace Vostok.Hercules.Client.Tests.Internal
{
    [TestFixture]
    public class LZ4HelperTests
    {
        [Test]
        public void Should_be_enabled()
        {
            LZ4Helper.Enabled.Should().BeTrue();
        }

        [Test]
        public void Should_not_throw()
        {
            LZ4Codec.MaximumOutputSize(42).Should().Be(58);
        }
    }
}
