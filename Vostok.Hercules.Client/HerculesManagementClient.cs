using System;
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
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesManagementClient : IHerculesManagementClient
    {
        private static readonly IResponseAnalyzer StreamAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Stream);
        private static readonly IResponseAnalyzer TimelineAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Timeline);

        private readonly JsonSerializer serializer = new JsonSerializer();

        private readonly HerculesManagementClientSettings settings;
        private readonly IClusterClient client;

        public HerculesManagementClient([NotNull] HerculesManagementClientSettings settings, [CanBeNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));

            log = (log ?? LogProvider.Get()).ForContext<HerculesManagementClient>();

            client = ClusterClientFactory.Create(settings.Cluster, log, Constants.ServiceNames.ManagementApi, settings.AdditionalSetup);
        }

        /// <inheritdoc />
        public Task<HerculesResult> CreateStreamAsync(CreateStreamQuery query, TimeSpan timeout)
            => CreateAsync("streams/create", query, timeout, StreamAnalyzer);

        /// <inheritdoc />
        public Task<HerculesResult> CreateTimelineAsync(CreateTimelineQuery query, TimeSpan timeout)
            => CreateAsync("timelines/create", query, timeout, TimelineAnalyzer);

        /// <inheritdoc />
        public Task<DeleteStreamResult> DeleteStreamAsync(string name, TimeSpan timeout)
            => DeleteAsync("streams/delete", Constants.QueryParameters.Stream, name, timeout, StreamAnalyzer, (status, error) => new DeleteStreamResult(status, error));

        /// <inheritdoc />
        public Task<DeleteTimelineResult> DeleteTimelineAsync(string name, TimeSpan timeout)
            => DeleteAsync("timelines/delete", Constants.QueryParameters.Timeline, name, timeout, TimelineAnalyzer, (status, error) => new DeleteTimelineResult(status, error));

        /// <inheritdoc />
        public Task<HerculesResult<StreamDescription>> GetStreamDescriptionAsync(string name, TimeSpan timeout) 
            => GetDescriptionAsync<StreamDescription>("streams/info", Constants.QueryParameters.Stream, name, timeout, StreamAnalyzer);

        /// <inheritdoc />
        public Task<HerculesResult<TimelineDescription>> GetTimelineDescriptionAsync(string name, TimeSpan timeout) 
            => GetDescriptionAsync<TimelineDescription>("timelines/info", Constants.QueryParameters.Timeline, name, timeout, TimelineAnalyzer);

        /// <inheritdoc />
        public Task<HerculesResult<string[]>> ListStreamsAsync(TimeSpan timeout) =>
            ListAsync("streams/list", timeout, StreamAnalyzer);

        /// <inheritdoc />
        public Task<HerculesResult<string[]>> ListTimelinesAsync(TimeSpan timeout) =>
            ListAsync("timelines/list", timeout, TimelineAnalyzer);
        
        private async Task<HerculesResult> CreateAsync(
            [NotNull] string path,
            [NotNull] object query, 
            TimeSpan timeout,
            [NotNull] IResponseAnalyzer analyzer)
        {
            var request = Request
                .Post(path)
                .WithHeader(Constants.HeaderNames.ApiKey, settings.ApiKeyProvider())
                .WithContentTypeHeader(Constants.ContentTypes.Json)
                .WithContent(serializer.Serialize(query));

            var result = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = analyzer.Analyze(result.Response, out var errorMessage);

            return new HerculesResult(status, errorMessage);
        }

        private async Task<TResult> DeleteAsync<TResult>(
            [NotNull] string path,
            [NotNull] string parameterName,
            [NotNull] string parameterValue,
            TimeSpan timeout,
            [NotNull] IResponseAnalyzer analyzer,
            [NotNull] Func<HerculesStatus, string, TResult> resultFactory)
        {
            var request = Request
                .Post(path)
                .WithHeader(Constants.HeaderNames.ApiKey, settings.ApiKeyProvider())
                .WithAdditionalQueryParameter(parameterName, parameterValue);

            var result = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = analyzer.Analyze(result.Response, out var error);

            return resultFactory(status, error);
        }

        private async Task<HerculesResult<TDescription>> GetDescriptionAsync<TDescription>(
            [NotNull] string path,
            [NotNull] string parameterName,
            [NotNull] string parameterValue,
            TimeSpan timeout,
            [NotNull] IResponseAnalyzer analyzer)
            where TDescription : class
        {
            var request = Request
                .Get(path)
                .WithHeader(Constants.HeaderNames.ApiKey, settings.ApiKeyProvider())
                .WithAdditionalQueryParameter(parameterName, parameterValue);

            var result = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = analyzer.Analyze(result.Response, out var errorMessage);

            var payload = status == HerculesStatus.Success
                ? serializer.Deserialize<TDescription>(result.Response.Content.ToArraySegment())
                : null;

            return new HerculesResult<TDescription>(status, payload, errorMessage);
        }

        private async Task<HerculesResult<string[]>> ListAsync(string path, TimeSpan timeout, IResponseAnalyzer analyzer)
        {
            var request = Request.Get(path)
                .WithHeader(Constants.HeaderNames.ApiKey, settings.ApiKeyProvider());

            var result = await client.SendAsync(request, timeout).ConfigureAwait(false);

            var status = analyzer.Analyze(result.Response, out var errorMessage);

            var payload = status == HerculesStatus.Success
                ? serializer.DeserializeStringArray(result.Response.Content.ToArraySegment())
                : null;

            return new HerculesResult<string[]>(status, payload, errorMessage);
        }
    }
}