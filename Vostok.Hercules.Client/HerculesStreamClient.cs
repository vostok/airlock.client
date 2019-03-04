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
using Vostok.Hercules.Client.Sending;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    public class HerculesStreamClient : IHerculesStreamClient
    {
        private const string ServiceName = "HerculesStreamApi";
        private const string ReadPath = "stream/read";
        
        private readonly ILog log;
        private readonly IClusterClient client;
        private readonly Func<string> getGateApiKey;

        public HerculesStreamClient(HerculesStreamClientSettings settings, ILog log)
        {
            this.log = log?.ForContext<HerculesStreamClient>() ?? new SilentLog();
            getGateApiKey = settings.ApiKeyProvider;

            client = new ClusterClient(
                log,
                configuration =>
                {
                    configuration.TargetServiceName = ServiceName;
                    configuration.ClusterProvider = settings.Cluster;
                    configuration.Transport = new UniversalTransport(this.log);
                    configuration.DefaultTimeout = 30.Seconds();
                    configuration.DefaultRequestStrategy = Strategy.Forking2;

                    configuration.SetupWeighedReplicaOrdering(builder => builder.AddAdaptiveHealthModifierWithLinearDecay(10.Minutes()));
                    configuration.SetupReplicaBudgeting(configuration.TargetServiceName);
                    configuration.SetupAdaptiveThrottling(configuration.TargetServiceName);
                });
        }

        public async Task<ReadStreamResult> ReadAsync(ReadStreamQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var apiKey = getGateApiKey();
                if (apiKey == null)
                {
                    log.Warn("Hercules API key is null.");
                    return new ReadStreamResult(HerculesStatus.Unauthorized, null);
                }

                var url = new RequestUrlBuilder(ReadPath)
                    .AppendToQuery(Constants.StreamQueryParameter, query.Name)
                    .AppendToQuery("take", query.Limit)
                    .AppendToQuery("shardIndex", query.ClientShard)
                    .AppendToQuery("shardCount", query.ClientShardCount)
                    .Build();

                var body = new BinaryBufferWriter(
                    sizeof(int) + query.Coordinates.Positions.Length * (sizeof(int) + sizeof(long)))
                {
                    Endianness = Endianness.Big
                };

                body.Write(query.Coordinates.Positions.Length);
                foreach (var position in query.Coordinates.Positions)
                {
                    body.Write(position.Partition);
                    body.Write(position.Offset);
                }

                var request = Request
                    .Post(url)
                    .WithHeader(HeaderNames.ContentType, Constants.OctetStreamContentType)
                    .WithHeader("apiKey", getGateApiKey())
                    .WithContent(body.FilledSegment);

                var result = await client
                    .SendAsync(request, timeout, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (result.Status != ClusterResultStatus.Success)
                    return new ReadStreamResult(ConvertFailureToHerculesStatus(result.Status), null);

                var response = result.Response;

                if (response.Code != ResponseCode.Ok)
                    return new ReadStreamResult(ConvertResponseCodeToHerculesStatus(response.Code), null);

                var reader = new BinaryBufferReader(response.Content.Buffer, response.Content.Offset)
                {
                    Endianness = Endianness.Big
                };

                var positions = new StreamPosition[reader.ReadInt32()];

                for (var i = 0; i < positions.Length; i++)
                {
                    positions[i] = new StreamPosition
                    {
                        Partition = reader.ReadInt32(),
                        Offset = reader.ReadInt64()
                    };
                }

                var events = new HerculesEvent[reader.ReadInt32()];

                for (var i = 0; i < events.Length; i++)
                {
                    events[i] = reader.ReadEvent();
                }

                return new ReadStreamResult(HerculesStatus.Success, new ReadStreamPayload(events, new StreamCoordinates(positions)));
            }
            catch (Exception e)
            {
                log.Warn(e);
                return new ReadStreamResult(HerculesStatus.UnknownError, null);
            }
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