using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Clusterclient.Core;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Ordering.Weighed;
using Vostok.Clusterclient.Core.Strategies;
using Vostok.Clusterclient.Transport;
using Vostok.Commons.Binary;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Abstractions.Values;
using Vostok.Hercules.Client.Sending;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    public class HerculesGateClient : IHerculesGateClient
    {
        private const string ServiceName = "HerculesGateway";
        private const int InitialBodyBufferSize = 4 * 1024;
        
        private readonly ILog log;
        private readonly IRequestSender sender;

        /// <inheritdoc />
        public HerculesGateClient(HerculesGateClientSettings settings, ILog log)
        {
            this.log = log = log?.ForContext<HerculesGateClient>() ?? new SilentLog();
            sender = new RequestSender(settings.Cluster, log, settings.ApiKeyProvider);
        }

        /// <inheritdoc />
        public async Task<InsertEventsResult> InsertAsync(
            InsertEventsQuery query,
            TimeSpan timeout,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var content = CreateContent(query);

                var result = await sender
                    .SendAsync(query.Stream, content, timeout, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (result.Status != ClusterResultStatus.Success)
                    return new InsertEventsResult(ConvertFailureToHerculesStatus(result.Status));

                if (result.Code != ResponseCode.Ok)
                    return new InsertEventsResult(ConvertResponseCodeToHerculesStatus(result.Code));

                return new InsertEventsResult(HerculesStatus.Success);
            }
            catch (Exception e)
            {
                log.Warn(e);
                return new InsertEventsResult(HerculesStatus.UnknownError);
            }
        }

        private static Content CreateContent(InsertEventsQuery query)
        {
            var body = new BinaryBufferWriter(InitialBodyBufferSize) {Endianness = Endianness.Big};

            body.Write(query.Events.Count);
            foreach (var @event in query.Events)
            {
                var eventBuilder = new HerculesEventBuilder(body, () => PreciseDateTime.UtcNow);
                eventBuilder
                    .SetTimestamp(@event.Timestamp)
                    .AddTags(@event.Tags);
            }

            return new Content(body.FilledSegment);
        }

        private static HerculesStatus ConvertFailureToHerculesStatus(ClusterResultStatus status)
        {
            switch (status)
            {
                case ClusterResultStatus.TimeExpired:
                    return HerculesStatus.Timeout;
                case ClusterResultStatus.Canceled:
                    return HerculesStatus.Canceled;
                case ClusterResultStatus.Throttled:
                    return HerculesStatus.Throttled;
                default:
                    return HerculesStatus.UnknownError;
            }
        }

        private static HerculesStatus ConvertResponseCodeToHerculesStatus(ResponseCode code)
        {
            switch (code)
            {
                case ResponseCode.RequestTimeout:
                    return HerculesStatus.Timeout;
                case ResponseCode.BadRequest:
                    return HerculesStatus.IncorrectRequest;
                case ResponseCode.NotFound:
                    return HerculesStatus.StreamNotFound;
                case ResponseCode.Unauthorized:
                    return HerculesStatus.Unauthorized;
                case ResponseCode.Forbidden:
                    return HerculesStatus.InsufficientPermissions;
                default:
                    return HerculesStatus.UnknownError;
            }
        }
    }
}