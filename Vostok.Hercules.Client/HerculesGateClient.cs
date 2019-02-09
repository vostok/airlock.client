using System;
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
using Vostok.Hercules.Client.Binary;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    public class HerculesGateClient : IHerculesGateClient
    {
        private readonly ILog log;
        private readonly IClusterClient client;
        private readonly Func<string> getGateApiKey;

        public HerculesGateClient(HerculesGateClientConfig config, ILog log)
        {
            this.log = log?.ForContext<HerculesGateClient>() ?? new SilentLog();
            getGateApiKey = config.ApiKeyProvider;

            client = new ClusterClient(
                log,
                configuration =>
                {
                    configuration.TargetServiceName = config.ServiceName ?? "HerculesGateway";
                    configuration.ClusterProvider = config.Cluster;
                    configuration.Transport = new UniversalTransport(log);
                    configuration.DefaultTimeout = 30.Seconds();
                    configuration.DefaultRequestStrategy = Strategy.Forking2;

                    configuration.SetupWeighedReplicaOrdering(builder => builder.AddAdaptiveHealthModifierWithLinearDecay(10.Minutes()));
                    configuration.SetupReplicaBudgeting(configuration.TargetServiceName);
                    configuration.SetupAdaptiveThrottling(configuration.TargetServiceName);
                });
        }

        public async Task<InsertEventsResult> InsertAsync(InsertEventsQuery query, TimeSpan timeout, CancellationToken cancellationToken = new CancellationToken())
        {
            try
            {
                var url = new RequestUrlBuilder("stream/send")
                    .AppendToQuery("stream", query.Stream)
                    .Build();

                var body = new BinaryBufferWriter(16 * 1024) {Endianness = Endianness.Big};

                body.Write(query.Events.Count);
                foreach (var @event in query.Events)
                {
                    var eventBuilder = new HerculesEventBuilder(body, () => PreciseDateTime.UtcNow);
                    eventBuilder.SetTimestamp(@event.Timestamp);
                    foreach (var tag in @event.Tags)
                    {
                        switch (tag.Value.Type)
                        {
                            case HerculesValueType.Byte:
                                eventBuilder.AddValue(tag.Key, tag.Value.AsByte);
                                break;
                            case HerculesValueType.Short:
                                eventBuilder.AddValue(tag.Key, tag.Value.AsShort);
                                break;
                            case HerculesValueType.Int:
                                eventBuilder.AddValue(tag.Key, tag.Value.AsInt);
                                break;
                            case HerculesValueType.Long:
                                eventBuilder.AddValue(tag.Key, tag.Value.AsShort);
                                break;
                            case HerculesValueType.Bool:
                                eventBuilder.AddValue(tag.Key, tag.Value.AsBool);
                                break;
                            case HerculesValueType.Float:
                                eventBuilder.AddValue(tag.Key, tag.Value.AsFloat);
                                break;
                            case HerculesValueType.Double:
                                eventBuilder.AddValue(tag.Key, tag.Value.AsDouble);
                                break;
                            case HerculesValueType.Guid:
                                eventBuilder.AddValue(tag.Key, tag.Value.AsGuid);
                                break;
                            case HerculesValueType.String:
                                eventBuilder.AddValue(tag.Key, tag.Value.AsString);
                                break;
                            case HerculesValueType.Vector:
                                var vector = tag.Value.AsVector;
                                switch (vector.ElementType)
                                {
                                    case HerculesValueType.Byte:
                                        eventBuilder.AddVector(tag.Key, vector.AsByteList);
                                        break;
                                    case HerculesValueType.Short:
                                        eventBuilder.AddVector(tag.Key, vector.AsShortList);
                                        break;
                                    case HerculesValueType.Int:
                                        eventBuilder.AddVector(tag.Key, vector.AsIntList);
                                        break;
                                    case HerculesValueType.Long:
                                        eventBuilder.AddVector(tag.Key, vector.AsLongList);
                                        break;
                                    case HerculesValueType.Bool:
                                        eventBuilder.AddVector(tag.Key, vector.AsBoolList);
                                        break;
                                    case HerculesValueType.Float:
                                        eventBuilder.AddVector(tag.Key, vector.AsFloatList);
                                        break;
                                    case HerculesValueType.Double:
                                        eventBuilder.AddVector(tag.Key, vector.AsDoubleList);
                                        break;
                                    case HerculesValueType.Guid:
                                        eventBuilder.AddVector(tag.Key, vector.AsGuidList);
                                        break;
                                    case HerculesValueType.String:
                                        eventBuilder.AddVector(tag.Key, vector.AsStringList);
                                        break;
                                    case HerculesValueType.Vector:
                                        break;
                                    case HerculesValueType.Container:
                                        //TODO
                                        break;
                                    default:
                                        throw new ArgumentOutOfRangeException();
                                }

                                break;
                            case HerculesValueType.Container:
                                //TODO
                                break;
                            default:
                                throw new ArgumentOutOfRangeException();
                        }
                    }
                }

                var request = Request
                    .Post(url)
                    .WithHeader(HeaderNames.ContentType, "application/octet-stream")
                    .WithContent(body.FilledSegment);

                var result = await client
                    .SendAsync(request, timeout, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);

                if (result.Status != ClusterResultStatus.Success)
                    return new InsertEventsResult(ConvertFailureToHerculesStatus(result.Status));

                var response = result.Response;

                if (response.Code != ResponseCode.Ok)
                    return new InsertEventsResult(ConvertResponseCodeToHerculesStatus(response.Code));

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

                return new InsertEventsResult(HerculesStatus.Success);
            }
            catch (Exception e)
            {
                log.Warn(e);
                return new InsertEventsResult(HerculesStatus.UnknownError);
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