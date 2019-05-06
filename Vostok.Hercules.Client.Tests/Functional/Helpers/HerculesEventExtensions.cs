using System.Collections.Generic;
using FluentAssertions;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client.Tests.Functional.Helpers
{
    internal static class HerculesEventExtensions
    {
        public static void ShouldBeEqual(this IEnumerable<HerculesEvent> actualEvents, IEnumerable<HerculesEvent> expectedEvents)
        {
            // FluentAssertions is slow on large sequences
            new HashSet<HerculesEvent>(actualEvents)
                .SetEquals(expectedEvents)
                .Should()
                .BeTrue();
        }
    }
}