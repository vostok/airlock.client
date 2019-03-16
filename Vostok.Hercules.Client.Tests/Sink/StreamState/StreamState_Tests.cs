using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;

namespace Vostok.Hercules.Client.Tests.Sink.StreamState
{
    [TestFixture]
    internal class StreamState_Tests
    {
        private Hercules.Client.Sink.StreamState.StreamState state;

        [SetUp]
        public void TestSetup()
        {
            state = new Hercules.Client.Sink.StreamState.StreamState("stream", Substitute.For<IBufferPool>(), Substitute.For<IStatisticsCollector>());
        }

        [Test]
        public void Should_initially_have_default_stream_settings()
        {
            state.Settings.Should().NotBeNull();
            state.Settings.ApiKeyProvider.Should().BeNull();
        }

        [Test]
        public void Should_initially_have_unset_send_signal()
        {
            state.SendSignal.WaitAsync().IsCompleted.Should().BeFalse();
        }
    }
}