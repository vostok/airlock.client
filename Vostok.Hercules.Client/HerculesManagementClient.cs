using System;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;
using Vostok.Hercules.Client.Management;
using Vostok.Hercules.Client.Utilities;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesManagementClient : IHerculesManagementClient
    {
        private readonly JsonSerializer serializer = new JsonSerializer();
        private readonly IResponseAnalyzer streamResponseAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Stream);
        private readonly IResponseAnalyzer timelineResponseAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Timeline);
        private ClusterClient client;
        private Func<string> getApiKey;

        /// <inheritdoc cref="HerculesManagementClient"/>
        public HerculesManagementClient(HerculesManagementClientSettings settings, ILog log)
        {
            client = ClusterClientFactory.Create(settings.Cluster, log, Constants.ServiceNames.ManagementApi, settings.ClusterClientSetup);
            getApiKey = settings.ApiKeyProvider;
        }

        /// <inheritdoc />
        public async Task<HerculesResult> CreateStreamAsync(CreateStreamQuery query, TimeSpan timeout)
        {
            var request = Request
                .Post("streams/create")
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey())
                .WithHeader(HeaderNames.ContentType, "application/json")
                .WithContent(serializer.Serialize(query));

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = streamResponseAnalyzer.Analyze(clusterResult.Response, out var errorMessage);

            return new HerculesResult(status, errorMessage);
        }

        /// <inheritdoc />
        public async Task<HerculesResult> CreateTimelineAsync(CreateTimelineQuery query, TimeSpan timeout)
        {
            var request = Request
                .Post("timelines/create")
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey())
                .WithContent(serializer.Serialize(query));

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = timelineResponseAnalyzer.Analyze(clusterResult.Response, out var errorMessage);

            return new HerculesResult(status, errorMessage);
        }

        /// <inheritdoc />
        public async Task<DeleteStreamResult> DeleteStreamAsync(string name, TimeSpan timeout)
        {
            var request = Request
                .Post("streams/delete")
                .WithAdditionalQueryParameter(Constants.QueryParameters.Stream, name)
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey());

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = streamResponseAnalyzer.Analyze(clusterResult.Response, out var errorMessage);

            return new DeleteStreamResult(status, errorMessage);
        }

        /// <inheritdoc />
        public async Task<DeleteTimelineResult> DeleteTimelineAsync(string name, TimeSpan timeout)
        {
            var request = Request
                .Post("timelines/delete")
                .WithAdditionalQueryParameter(Constants.QueryParameters.Timeline, name)
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey());

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = timelineResponseAnalyzer.Analyze(clusterResult.Response, out var errorMessage);

            return new DeleteTimelineResult(status, errorMessage);
        }

        public async Task<HerculesResult<StreamDescription>> GetStreamDescriptionAsync(string name, TimeSpan timeout)
        {
            var request = Request
                .Get("streams/info")
                .WithAdditionalQueryParameter(Constants.QueryParameters.Stream, name)
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey());

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var response = clusterResult.Response;

            var status = streamResponseAnalyzer.Analyze(response, out var errorMessage);

            var payload = status == HerculesStatus.Success
                ? serializer
                    .DeserializeStreamDescription(response.Content.ToArraySegment())
                : null;

            return new HerculesResult<StreamDescription>(status, payload, errorMessage);
        }

        public async Task<HerculesResult<TimelineDescription>> GetTimelineDescriptionAsync(string name, TimeSpan timeout)
        {
            var request = Request
                .Get("timelines/info")
                .WithAdditionalQueryParameter(Constants.QueryParameters.Timeline, name)
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey());

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var response = clusterResult.Response;

            var status = timelineResponseAnalyzer.Analyze(response, out var errorMessage);

            var payload = status == HerculesStatus.Success
                ? serializer.DeserializeTimelineDescription(response.Content.ToArraySegment())
                : null;

            return new HerculesResult<TimelineDescription>(status, payload, errorMessage);
        }

        public async Task<HerculesResult<string[]>> ListStreamsAsync(TimeSpan timeout)
        {
            var request = Request
                .Get("streams/list")
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey());

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var response = clusterResult.Response;

            var status = streamResponseAnalyzer.Analyze(response, out var errorMessage);

            var payload = status == HerculesStatus.Success
                ? serializer.DeserializeStringArray(response.Content.ToArraySegment())
                : null;

            return new HerculesResult<string[]>(status, payload, errorMessage);
        }

        public async Task<HerculesResult<string[]>> ListTimelinesAsync(TimeSpan timeout)
        {
            var request = Request
                .Get("timelines/list")
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey());

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var response = clusterResult.Response;

            var status = timelineResponseAnalyzer.Analyze(response, out var errorMessage);

            var payload = status == HerculesStatus.Success
                ? serializer.DeserializeStringArray(response.Content.ToArraySegment())
                : null;

            return new HerculesResult<string[]>(status, payload, errorMessage);
        }
    }
}