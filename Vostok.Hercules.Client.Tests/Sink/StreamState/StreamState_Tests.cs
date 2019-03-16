using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Statistics;
using Vostok.Hercules.Client.Sink.StreamState;
using Vostok.Hercules.Client.Sink.Writing;

namespace Vostok.Hercules.Client.Tests.Sink.StreamState
{
    [TestFixture]
    internal class StreamState_Tests
    {
        private Hercules.Client.Sink.StreamState.StreamState state;

        [SetUp]
        public void TestSetup()
        {
            state = new Hercules.Client.Sink.StreamState.StreamState(
                "stream", 
                Substitute.For<IBufferPool>(),
                Substitute.For<IRecordWriter>(),
                Substitute.For<IStatisticsCollector>(),
                new AsyncManualResetEvent(false));
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