using System;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Gate;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Daemon;
using Vostok.Hercules.Client.Sink.Planner;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.Sending;
using Vostok.Hercules.Client.Sink.Statistics;
using Vostok.Hercules.Client.Sink.StreamState;
using Vostok.Hercules.Client.Sink.Writing;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc cref="IHerculesSink" />
    [PublicAPI]
    public class HerculesSink : IHerculesSink, IDisposable
    {
        private readonly HerculesSinkSettings settings;
        private readonly IRecordWriter recordWriter;
        private readonly IRecordsSendingDaemon sendingDaemon;
        private readonly IStreamStateFactory stateFactory;
        private readonly ILog log;

        private readonly ConcurrentDictionary<string, Lazy<IStreamState>> streamStates;
        private readonly AtomicBoolean isDisposed;

        public HerculesSink([NotNull] HerculesSinkSettings settings, [CanBeNull] ILog log)
            : this(settings, null, null, null, log)
        {
            recordWriter = new RecordWriter(this.log, () => PreciseDateTime.UtcNow, Constants.ProtocolVersion, settings.MaximumRecordSize);

            var memoryManager = new MemoryManager(settings.MaximumMemoryConsumption);
            var batcher = new BufferSnapshotBatcher(settings.MaximumBatchSize);
            var contentFactory = new RequestContentFactory();
            var requestSender = new GateRequestSender(settings.Cluster, this.log, settings.ApiKeyProvider, settings.ClusterClientSetup);
            var senderFactory = new StreamSenderFactory(batcher, contentFactory, requestSender, this.log);
            var plannerFactory = new PlannerFactory(settings);
            var scheduler = new Scheduler(this, streamStates, settings, senderFactory, plannerFactory);

            stateFactory = new StreamStateFactory(settings, memoryManager);
            sendingDaemon = new RecordsSendingDaemon(this.log, scheduler);
        }

        internal HerculesSink(
            HerculesSinkSettings settings,
            IRecordWriter recordWriter,
            IRecordsSendingDaemon sendingDaemon,
            IStreamStateFactory stateFactory,
            ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = (log ?? LogProvider.Get()).ForContext<HerculesSink>();

            this.recordWriter = recordWriter;
            this.sendingDaemon = sendingDaemon;
            this.stateFactory = stateFactory;

            streamStates = new ConcurrentDictionary<string, Lazy<IStreamState>>();
            isDisposed = new AtomicBoolean(false);
        }

        /// <inheritdoc />
        public void Put(string stream, Action<IHerculesEventBuilder> build)
        {
            if (isDisposed)
            {
                LogDisposed();
                return;
            }

            if (!ValidateParameters(stream, build))
                return;

            var streamState = ObtainStreamState(stream);
            var statistics = streamState.Statistics;
            var bufferPool = streamState.BufferPool;

            if (!bufferPool.TryAcquire(out var buffer))
            {
                statistics.ReportOverflow();
                return;
            }

            try
            {
                WriteRecord(build, statistics, buffer, streamState.SendSignal);
            }
            finally
            {
                bufferPool.Release(buffer);
            }

            sendingDaemon.Initialize();
        }

        /// <summary>
        /// <para>Provides diagnostics information about <see cref="HerculesSink"/>.</para>
        /// </summary>
        public HerculesSinkStatistics GetStatistics()
        {
            var perStreamCounters = streamStates
                .Where(x => x.Value.IsValueCreated)
                .ToDictionary(x => x.Key, x => x.Value.Value.Statistics.Get());

            var totalCounters = HerculesSinkCounters.Zero;

            foreach (var value in perStreamCounters.Values)
                totalCounters += value;

            return new HerculesSinkStatistics(totalCounters, perStreamCounters);
        }

        /// <inheritdoc />
        public void ConfigureStream(string stream, StreamSettings streamSettings)
            => ObtainStreamState(stream).Settings = streamSettings ?? throw new ArgumentNullException(nameof(streamSettings));

        /// <inheritdoc />
        public void Dispose()
        {
            if (isDisposed.TrySetTrue())
                sendingDaemon.Dispose();
        }

        private void WriteRecord(Action<IHerculesEventBuilder> build, IStatisticsCollector statistics, IBuffer buffer, AsyncManualResetEvent sendSignal)
        {
            var storedSizeBefore = statistics.EstimateStoredSize();

            switch (recordWriter.TryWrite(buffer, build, out var recordSize))
            {
                case RecordWriteResult.Success:
                    statistics.ReportStoredRecord(recordSize);
                    var storedSizeAfter = statistics.EstimateStoredSize();
                    var threshold = settings.MaximumPerStreamMemoryConsumption / 4;
                    if (storedSizeBefore < threshold && threshold <= storedSizeAfter)
                        sendSignal.Set();
                    break;
                case RecordWriteResult.Exception:
                    statistics.ReportRecordBuildFailure();
                    break;
                case RecordWriteResult.OutOfMemory:
                    statistics.ReportOverflow();
                    break;
                case RecordWriteResult.RecordTooLarge:
                    statistics.ReportSizeLimitViolation();
                    break;
            }
        }

        private void LogDisposed()
        {
            log.Warn("An attempt to put event to disposed HerculesSink.");
        }

        private bool ValidateParameters(string stream, Action<IHerculesEventBuilder> build)
        {
            if (string.IsNullOrEmpty(stream))
            {
                log.Warn("An attempt to put event to stream which name is null or empty.");
                return false;
            }

            // ReSharper disable once InvertIf
            if (build == null)
            {
                log.Warn("A delegate that provided to build an event is null.");
                return false;
            }

            return true;
        }

        [NotNull]
        private IStreamState ObtainStreamState([NotNull] string stream) =>
            streamStates
                .GetOrAdd(stream, s => new Lazy<IStreamState>(() => stateFactory.Create(s)))
                .Value;
    }
}
