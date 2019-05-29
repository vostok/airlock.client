using System;
using System.Collections.Concurrent;
using FluentAssertions;
using NSubstitute;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.State;

// ReSharper disable ReturnValueOfPureMethodIsNotUsed

namespace Vostok.Hercules.Client.Tests.Sink.State
{
    [TestFixture]
    internal class StreamStatesProvider_Tests
    {
        [Test]
        public void Should_only_return_already_initialized_states()
        {
            var states = new ConcurrentDictionary<string, Lazy<IStreamState>>
            {
                ["stream1"] = new Lazy<IStreamState>(() => Substitute.For<IStreamState>()),
                ["stream2"] = new Lazy<IStreamState>(() => Substitute.For<IStreamState>()),
                ["stream3"] = new Lazy<IStreamState>(() => Substitute.For<IStreamState>()),
                ["stream4"] = new Lazy<IStreamState>(() => Substitute.For<IStreamState>()),
                ["stream5"] = new Lazy<IStreamState>(() => Substitute.For<IStreamState>())
            };

            states["stream1"].Value.GetHashCode();
            states["stream3"].Value.GetHashCode();
            states["stream5"].Value.GetHashCode();

            var provider = new StreamStatesProvider(states);

            provider.GetStates().Should().HaveCount(3);
            provider.GetStates().Should().HaveCount(3);
        }
    }
}