using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Hercules.Client.Sink.Analyzer;

namespace Vostok.Hercules.Client.Tests.Sink.Analyzer
{
    [TestFixture]
    internal class MemoryAnalyzer_Tests
    {
        [Test]
        public void ShouldFreeMemory_should_return_true_if_period_elapsed()
        {
            // TODO(kungurtsev): 
            //var analyzer = new MemoryAnalyzer(1.Minutes());
            //analyzer.ShouldFreeMemory(DateTime.UtcNow.Ticks - 50.Seconds().Ticks).Should().BeFalse();
            //analyzer.ShouldFreeMemory(DateTime.UtcNow.Ticks - 70.Seconds().Ticks).Should().BeTrue();
        }
    }
}