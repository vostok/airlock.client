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
using Vostok.Hercules.Client.Serialization.Json;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesManagementClient : IHerculesManagementClient
    {
        private static readonly IResponseAnalyzer StreamAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Stream);
        private static readonly IResponseAnalyzer TimelineAnalyzer = new ResponseAnalyzer(ResponseAnalysisContext.Timeline);

        private readonly IJsonSerializer serializer;
        private readonly IClusterClient client;
        private readonly ILog log;

        public HerculesManagementClient([NotNull] HerculesManagementClientSettings settings, [CanBeNull] ILog log)
            : this(settings, JsonSerializer.Instance, CreateClient(settings, log), log)
        {
        }

        internal HerculesManagementClient(
            [NotNull] HerculesManagementClientSettings settings,
            [NotNull] IJsonSerializer serializer,
            [NotNull] IClusterClient client,
            [CanBeNull] ILog log)
        {
            this.log = log = (log ?? LogProvider.Get()).ForContext<HerculesManagementClient>();

            this.client = client;

            this.serializer = serializer;
        }

        /// <inheritdoc />
        public Task<HerculesResult> CreateStreamAsync(CreateStreamQuery query, TimeSpan timeout)
            => SendAsync(
                Request.Post("streams/create"),
                StreamDescriptionDtoConverter.CreateFromQuery(query),
                timeout,
                StreamAnalyzer);

        /// <inheritdoc />
        public Task<HerculesResult> CreateTimelineAsync(CreateTimelineQuery query, TimeSpan timeout)
            => SendAsync(
                Request.Post("timelines/create"),
                TimelineDescriptionDtoConverter.CreateFromQuery(query),
                timeout,
                TimelineAnalyzer);

        /// <inheritdoc />
        public Task<DeleteStreamResult> DeleteStreamAsync(string name, TimeSpan timeout)
            => SendAsync(
                Request.Post("streams/delete").WithStreamName(name),
                timeout,
                StreamAnalyzer,
                result => new DeleteStreamResult(result.Status, result.ErrorDetails));

        /// <inheritdoc />
        public Task<DeleteTimelineResult> DeleteTimelineAsync(string name, TimeSpan timeout)
            => SendAsync(
                Request.Post("timelines/delete").WithTimelineName(name),
                timeout,
                TimelineAnalyzer,
                result => new DeleteTimelineResult(result.Status, result.ErrorDetails));

        /// <inheritdoc />
        public Task<HerculesResult<string[]>> ListStreamsAsync(TimeSpan timeout)
            => SendAsync<string[]>(Request.Get("streams/list"), timeout, StreamAnalyzer);

        /// <inheritdoc />
        public Task<HerculesResult<string[]>> ListTimelinesAsync(TimeSpan timeout)
            => SendAsync<string[]>(Request.Get("timelines/list"), timeout, TimelineAnalyzer);

        /// <inheritdoc />
        public Task<HerculesResult<StreamDescription>> GetStreamDescriptionAsync(string name, TimeSpan timeout)
            => SendAsync<StreamDescriptionDto, StreamDescription>(
                Request.Get("streams/info").WithStreamName(name),
                timeout,
                StreamAnalyzer,
                StreamDescriptionDtoConverter.ConvertToDescription);

        /// <inheritdoc />
        public Task<HerculesResult<TimelineDescription>> GetTimelineDescriptionAsync(string name, TimeSpan timeout)
            => SendAsync<TimelineDescriptionDto, TimelineDescription>(
                Request.Get("timelines/info").WithTimelineName(name),
                timeout,
                TimelineAnalyzer,
                TimelineDescriptionDtoConverter.ConvertToDescription);

        private Task<TResult> SendAsync<TResult>(Request request, TimeSpan timeout, IResponseAnalyzer analyzer, Func<HerculesResult, TResult> resultFactory)
            => SendAsync(request, null, timeout, analyzer).ContinueWith(task => resultFactory(task.GetAwaiter().GetResult()));

        private Task<HerculesResult<TPayload>> SendAsync<TPayload>(Request request, TimeSpan timeout, IResponseAnalyzer analyzer)
            => SendAsync<TPayload, TPayload>(request, timeout, analyzer, _ => _);

        private async Task<HerculesResult> SendAsync(Request request, object requestDto, TimeSpan timeout, IResponseAnalyzer analyzer)
        {
            if (requestDto != null)
            {
                request = request.WithContentTypeHeader(Constants.ContentTypes.Json);
                request = request.WithContent(serializer.Serialize(requestDto));
            }

            var result = await client.SendAsync(request, timeout).ConfigureAwait(false);
            var status = analyzer.Analyze(result.Response, out var errorMessage);

            return new HerculesResult(status, errorMessage);
        }

        private async Task<HerculesResult<TPayload>> SendAsync<TDto, TPayload>(Request request, TimeSpan timeout, IResponseAnalyzer analyzer, Func<TDto, TPayload> converter)
        {
            var result = await client.SendAsync(request, timeout).ConfigureAwait(false);
            var payload = default(TPayload);

            var status = analyzer.Analyze(result.Response, out var errorMessage);
            if (status == HerculesStatus.Success)
                payload = converter(serializer.Deserialize<TDto>(result.Response.Content.ToMemoryStream()));

            return new HerculesResult<TPayload>(status, payload, errorMessage);
        }

        private static IClusterClient CreateClient(HerculesManagementClientSettings settings, ILog log)
        {
            return ClusterClientFactory.Create(
                settings.Cluster,
                log,
                Constants.ServiceNames.ManagementApi,
                config =>
                {
                    config.AddRequestTransform(new ApiKeyRequestTransform(settings.ApiKeyProvider));
                    settings.AdditionalSetup?.Invoke(config);
                });
        }
    }
}