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
        private readonly IResponseAnalyzer responseAnalyzer = new ResponseAnalyzer();
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

            var status = responseAnalyzer.Analyze(clusterResult.Response, out var errorMessage);

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

            var status = responseAnalyzer.Analyze(clusterResult.Response, out var errorMessage, ResponseAnalysisContext.Timeline);

            return new HerculesResult(status, errorMessage);
        }


        private async Task<HerculesResult> CreateAsync(CreateTimelineQuery query, TimeSpan timeout, ResponseAnalysisContext context)
        {
            var url = context == ResponseAnalysisContext.Stream
                ? "streams/create"
                : "timelines/create";

            var request = Request
                .Post(url)
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey())
                .WithContent(serializer.Serialize(query));

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = responseAnalyzer.Analyze(clusterResult.Response, out var errorMessage, context);

            return new HerculesResult(status, errorMessage);
        }

        /// <inheritdoc />
        public async Task<DeleteStreamResult> DeleteStreamAsync(string name, TimeSpan timeout)
        {
            var result = await DeleteAsync(name, timeout, ResponseAnalysisContext.Stream).ConfigureAwait(false);
            return new DeleteStreamResult(result.status, result.errorMessage);
        }

        /// <inheritdoc />
        public async Task<DeleteTimelineResult> DeleteTimelineAsync(string name, TimeSpan timeout)
        {
            var result = await DeleteAsync(name, timeout, ResponseAnalysisContext.Timeline).ConfigureAwait(false);
            return new DeleteTimelineResult(result.status, result.errorMessage);
        }
        
        private async Task<(HerculesStatus status, string errorMessage)> DeleteAsync(string name, TimeSpan timeout, ResponseAnalysisContext context)
        {
            var (url, queryParam) = context == ResponseAnalysisContext.Stream
                ? ("streams/delete", Constants.QueryParameters.Stream)
                : ("timelines/delete", Constants.QueryParameters.Timeline);

            var request = Request
                .Post(url)
                .WithAdditionalQueryParameter(queryParam, name)
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey());

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = responseAnalyzer.Analyze(clusterResult.Response, out var errorMessage, ResponseAnalysisContext.Timeline);

            return (status, errorMessage);
        }

        public Task<HerculesResult<StreamDescription>> GetStreamDescriptionAsync(string name, TimeSpan timeout) =>
            GetDescriptionAsync<StreamDescription>(name, timeout);

        public Task<HerculesResult<TimelineDescription>> GetTimelineDescriptionAsync(string name, TimeSpan timeout) =>
            GetDescriptionAsync<TimelineDescription>(name, timeout);

        private async Task<HerculesResult<TModel>> GetDescriptionAsync<TModel>(string name, TimeSpan timeout)
            where TModel : class
        {
            var (context, url, queryParam) = typeof(TModel) == typeof(StreamDescription)
                ? (ResponseAnalysisContext.Stream, "streams/info", Constants.QueryParameters.Stream)
                : (ResponseAnalysisContext.Timeline, "timelines/info", Constants.QueryParameters.Timeline);
            
            var request = Request
                .Get(url)
                .WithAdditionalQueryParameter(queryParam, name)
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey());

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var response = clusterResult.Response;

            var status = responseAnalyzer.Analyze(response, out var errorMessage, context);

            var payload = status == HerculesStatus.Success
                ? serializer.Deserialize<TModel>(response.Content.ToArraySegment())
                : null;

            return new HerculesResult<TModel>(status, payload, errorMessage);
        }

        public Task<HerculesResult<string[]>> ListStreamsAsync(TimeSpan timeout) =>
            ListAsync(timeout, ResponseAnalysisContext.Stream);

        public Task<HerculesResult<string[]>> ListTimelinesAsync(TimeSpan timeout) =>
            ListAsync(timeout, ResponseAnalysisContext.Timeline);
        

        private async Task<HerculesResult<string[]>> ListAsync(TimeSpan timeout, ResponseAnalysisContext context)
        {
            var url = context == ResponseAnalysisContext.Stream
                ? "streams/list"
                : "timelines/list";
            
            var request = Request
                .Get(url)
                .WithHeader(Constants.HeaderNames.ApiKey, getApiKey());

            var clusterResult = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var response = clusterResult.Response;

            var status = responseAnalyzer.Analyze(response, out var errorMessage, context);

            var payload = status == HerculesStatus.Success
                ? serializer.DeserializeStringArray(response.Content.ToArraySegment())
                : null;

            return new HerculesResult<string[]>(status, payload, errorMessage);
        }
    }
}