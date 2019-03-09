using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Gateway;
using Vostok.Hercules.Client.Sink;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Daemon;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.Sending;
using Vostok.Hercules.Client.Sink.Writing;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc cref="IHerculesSink" />
    [PublicAPI]
    public class HerculesSink : IHerculesSink, IDisposable
    {
        private readonly HerculesSinkSettings settings;

        private readonly ILog log;

        private readonly IHerculesRecordWriter recordWriter;

        private readonly IRecordsSendingDaemon recordsSendingDaemon;

        private readonly IStreamContextFactory streamContextFactory;

        private readonly ConcurrentDictionary<string, Lazy<StreamContext>> streamStates;

        private int isDisposed;

        /// <summary>
        /// Creates a new instance of <see cref="HerculesSink"/>.
        /// </summary>
        /// <param name="settings">Settings of this <see cref="HerculesSink"/></param>
        /// <param name="log">An <see cref="ILog"/> instance.</param>
        public HerculesSink(HerculesSinkSettings settings, ILog log)
        {
            this.settings = settings;
            this.log = log = (log ?? LogProvider.Get()).ForContext<HerculesSink>();

            recordWriter = new HerculesRecordWriter(log, () => PreciseDateTime.UtcNow, Constants.ProtocolVersion, settings.MaximumRecordSize);

            streamStates = new ConcurrentDictionary<string, Lazy<StreamContext>>();

            var memoryManager = new MemoryManager(settings.MaximumMemoryConsumption);

            var requestSender = new RequestSender(settings.Cluster, log, settings.ApiKeyProvider, settings.ClusterClientSetup);

            var batcher = new BufferSnapshotBatcher(settings.MaximumBatchSize);

            var contentFactory = new RequestContentFactory();

            streamContextFactory = new StreamContextFactory(settings, batcher, contentFactory, requestSender, memoryManager, log);

            var job = new RecordsSendingJob(
                this,
                streamStates,
                settings.RequestTimeout);

            recordsSendingDaemon = new RecordsSendingDaemon(log, job);
        }

        /// <inheritdoc />
        public void Put(string stream, Action<IHerculesEventBuilder> build)
        {
            if (IsDisposed())
            {
                LogDisposed();
                return;
            }

            if (!ValidateParameters(stream, build))
                return;

            var streamContext = GetOrCreate(stream);
            var statistics = streamContext.State.Statistics;
            var bufferPool = streamContext.State.BufferPool;
            var scheduler = streamContext.Sender;

            if (!bufferPool.TryAcquire(out var buffer))
            {
                statistics.ReportOverflow();
                return;
            }

            try
            {
                WriteRecord(build, statistics, buffer, scheduler);
            }
            finally
            {
                bufferPool.Release(buffer);
            }

            recordsSendingDaemon.Initialize();
        }

        /// <summary>
        /// <para>Provides diagnostics information about <see cref="HerculesSink"/>.</para>
        /// </summary>
        public HerculesSinkStatistics GetStatistics()
        {
            var stats = new HerculesSinkStatistics();

            var streamsStats = streamStates
                .Where(x => x.Value.IsValueCreated)
                .Select(x => x.Value.Value.State.Statistics.Get());

            foreach (var streamStats in streamsStats)
            {
                stats.LostRecords = Sum(stats.LostRecords, streamStats.LostRecords);
                stats.SentRecords = Sum(stats.SentRecords, streamStats.SentRecords);
                stats.StoredRecords = Sum(stats.SentRecords, streamStats.StoredRecords);
                stats.OverflowsCount += streamStats.OverflowsCount;
                stats.WriteFailuresCount += streamStats.WriteFailuresCount;
                stats.TooLargeRecordsCount += streamStats.TooLargeRecordsCount;
            }

            return stats;
        }

        /// <inheritdoc />
        public void ConfigureStream(string stream, StreamSettings settings)
        {
            var streamState = GetOrCreate(stream).State;
            streamState.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 0)
                recordsSendingDaemon.Dispose();
        }

        private (long, long) Sum((long, long) a, (long, long) b) => (a.Item1 + b.Item1, a.Item2 + b.Item2);

        private void WriteRecord(Action<IHerculesEventBuilder> build, IStatisticsCollector statistics, IBuffer buffer, ISchedulingStreamSender sender)
        {
            var storedSizeBefore = statistics.EstimateStoredSize();

            switch (recordWriter.TryWrite(buffer, build, out var recordSize))
            {
                case WriteResult.NoError:
                    buffer.Commit(recordSize);
                    statistics.ReportWrittenRecord(recordSize);
                    var storedSizeAfter = statistics.EstimateStoredSize();
                    var threshold = settings.MaximumPerStreamMemoryConsumption / 4;
                    if (storedSizeBefore < threshold && threshold <= storedSizeAfter)
                        sender.Wakeup();
                    break;
                case WriteResult.Exception:
                    statistics.ReportWriteFailure();
                    break;
                case WriteResult.OutOfMemory:
                    statistics.ReportOverflow();
                    break;
                case WriteResult.RecordTooLarge:
                    statistics.ReportTooLargeRecord();
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

        private StreamContext GetOrCreate(string stream) =>
            streamStates.GetOrAdd(
                    stream,
                    s => new Lazy<StreamContext>(() => streamContextFactory.Create(s)))
                .Value;

        private bool IsDisposed()
        {
            return isDisposed == 1;
        }
    }
}