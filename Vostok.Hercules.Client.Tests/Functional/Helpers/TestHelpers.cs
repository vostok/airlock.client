using System;
using System.Linq;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client.Tests.Functional.Helpers
{
    internal static class TestHelpers
    {
        public static string GenerateStreamName() => $"dotnet_test_csharpclient_{Guid.NewGuid().ToString().Substring(0, 8)}";
        public static Action<IHerculesEventBuilder>[] GenerateEventBuilders(int count, Action<IHerculesEventBuilder> eventCustomization = null)
        {
            var timestamp = DateTimeOffset.UtcNow;

            return Enumerable
                .Range(0, count)
                .Select(
                    i => new Action<IHerculesEventBuilder>(
                        x =>
                        {
                            x
                                .SetTimestamp(timestamp)
                                .AddValue("x", i);
                            eventCustomization?.Invoke(x);
                        }))
                .ToArray();
        }
    }
}