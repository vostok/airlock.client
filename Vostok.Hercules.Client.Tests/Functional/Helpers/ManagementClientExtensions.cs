using System;
using FluentAssertions.Extensions;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Queries;

namespace Vostok.Hercules.Client.Tests.Functional.Helpers
{
    internal static class ManagementClientExtensions
    {
        private static readonly TimeSpan Timeout = 20.Seconds();

        public static IDisposable CreateTemporaryStream(this IHerculesManagementClient client, out string name, int? partitions = null, string[] shardingKey = null)
        {
            var streamName = name = TestHelpers.GenerateStreamName();

            var createStreamQuery = new CreateStreamQuery(name)
            {
                ShardingKey = shardingKey,
                Partitions = partitions
            };

            client.CreateStream(createStreamQuery, Timeout).EnsureSuccess();

            return new Disposable(() => client.DeleteStream(streamName, Timeout));
        }
    }
}