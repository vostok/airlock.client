﻿using System;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Commons.Binary;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Queries;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Serialization.Builders;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc />
    [PublicAPI]
    public class HerculesGateClient : IHerculesGateClient
    {
        private const string ServiceName = "HerculesGateway";
        private const int InitialBodyBufferSize = 4096;

        private readonly HerculesGateClientSettings settings;
        private readonly IGateRequestSender sender;
        private readonly ILog log;

        /// <inheritdoc />
        public HerculesGateClient(HerculesGateClientSettings settings, ILog log)
        {
            this.settings = settings;
            this.log = log = (log?? LogProvider.Get()).ForContext<HerculesGateClient>();

            sender = new GateRequestSender(settings.Cluster, log, null);
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
                    .SendAsync(query.Stream, settings.ApiKeyProvider(), content, timeout, cancellationToken)
                    .ConfigureAwait(false);

                if (result.Status != ClusterResultStatus.Success)
                    return new InsertEventsResult(ConvertFailureToHerculesStatus(result.Status));

                return result.Code == ResponseCode.Ok
                    ? new InsertEventsResult(HerculesStatus.Success)
                    : new InsertEventsResult(ConvertResponseCodeToHerculesStatus(result.Code));
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
                using (var eventBuilder = new BinaryEventBuilder(body, () => PreciseDateTime.UtcNow, Constants.EventProtocolVersion))
                {
                    eventBuilder
                        .SetTimestamp(@event.Timestamp)
                        .AddTags(@event.Tags);
                }
            }

            return new Content(body.FilledSegment);
        }

        private static HerculesStatus ConvertFailureToHerculesStatus(ClusterResultStatus status)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
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
            // ReSharper disable once SwitchStatementMissingSomeCases
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