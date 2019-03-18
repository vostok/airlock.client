using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Topology;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Management;
using Vostok.Hercules.Client.Serialization.Json;
using Vostok.Logging.Console;

namespace Vostok.Hercules.Client.Tests
{
    internal class HerculesManagementClient_Tests
    {
        private IJsonSerializer serializer;
        private HerculesManagementClient client;
        private IClusterClient clusterClient;

        [SetUp]
        public void Setup()
        {
            var settings = new HerculesManagementClientSettings(Substitute.For<IClusterProvider>(), () => "");
            serializer = Substitute.For<IJsonSerializer>();
            clusterClient = Substitute.For<IClusterClient>();
            client = new HerculesManagementClient(settings, serializer, clusterClient, new SynchronousConsoleLog());

            var result = new ClusterResult(
                ClusterResultStatus.Success,
                new[]
                {
                    new ReplicaResult(new Uri("http://a/"), Responses.Ok, ResponseVerdict.Accept, default)
                },
                Responses.Ok,
                Request.Get("http:/a/"));

            clusterClient.SendAsync(default).ReturnsForAnyArgs(Task.FromResult<ClusterResult>(result));
        }

        [TestCaseSource(nameof(Calls))]
        public void Should_return_UnknownError_when_serializer_throws_exception(Func<HerculesManagementClient, HerculesResult> func)
        {
            SetupSerializerExceptions();

            AssertUnknownError(func);
        }

        private static IEnumerable<Func<HerculesManagementClient, HerculesResult>> Calls()
        {
            var streamQuery = new CreateStreamQuery("name");
            var timelineQuery = new CreateTimelineQuery("name", new[] {"sources"});

            var timeout = 5.Seconds();

            yield return c => c.CreateStream(streamQuery, timeout);
            yield return c => c.CreateTimeline(timelineQuery, timeout);
            yield return c => c.ListStreams(timeout);
            yield return c => c.ListTimelines(timeout);
            yield return c => c.GetStreamDescription("name", timeout);
            yield return c => c.GetTimelineDescription("name", timeout);
        }

        private void AssertUnknownError(Func<HerculesManagementClient, HerculesResult> action)
        {
            var result = action(client);
            result.Status.Should().Be(HerculesStatus.UnknownError);
            result.ErrorDetails.Should().NotBeNullOrEmpty();
        }

        private void SetupSerializerExceptions()
        {
            var exception = new Exception();
            serializer.Serialize(null).ThrowsForAnyArgs(exception);
            serializer.Deserialize<StreamDescriptionDto>(null).ThrowsForAnyArgs(exception);
            serializer.Deserialize<TimelineDescriptionDto>(null).ThrowsForAnyArgs(exception);
            serializer.Deserialize<string[]>(null).ThrowsForAnyArgs(exception);
        }
    }
}