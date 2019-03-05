using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using Vostok.Commons.Primitives;
using Vostok.Commons.Time;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Gateway;
using Vostok.Hercules.Client.Sink;
using Vostok.Hercules.Client.Sink.Buffers;
using Vostok.Hercules.Client.Sink.Requests;
using Vostok.Hercules.Client.Sink.Worker;
using Vostok.Hercules.Client.Sink.Writing;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc cref="IHerculesSink" />
    [PublicAPI]
    public class HerculesSink : IHerculesSink, IDisposable
    {
        private const int InitialPooledBufferSize = 4 * (int)DataSizeConstants.Kilobyte;

        private readonly ILog log;

        private readonly IHerculesRecordWriter recordWriter;
        private readonly IMemoryManager memoryManager;

        private readonly int initialPooledBuffersCount;
        private readonly int initialPooledBufferSize;
        private readonly ConcurrentDictionary<string, Lazy<IBufferPool>> bufferPools;

        private readonly HerculesRecordsSendingDaemon recordsSendingDaemon;

        private int isDisposed;
        private long lostRecordsCounter;
        private int maximumRecordSize;
        private int maximumBatchSize;
        private long maximumPerStreamMemoryConsumptionBytes;

        /// <summary>
        /// Creates a new instance of <see cref="HerculesSink"/>.
        /// </summary>
        /// <param name="settings">Settings of this <see cref="HerculesSink"/></param>
        /// <param name="log">An <see cref="ILog"/> instance.</param>
        public HerculesSink(HerculesSinkSettings settings, ILog log)
        {
            log = (log ?? LogProvider.Get()).ForContext<HerculesSink>();

            recordWriter = new HerculesRecordWriter(log, () => PreciseDateTime.UtcNow, Constants.ProtocolVersion, settings.MaximumRecordSize);

            memoryManager = new MemoryManager(settings.MaximumMemoryConsumption);

            initialPooledBufferSize = InitialPooledBufferSize;
            maximumRecordSize = settings.MaximumRecordSize;
            maximumBatchSize = settings.MaximumBatchSize;
            maximumPerStreamMemoryConsumptionBytes = settings.MaximumPerStreamMemoryConsumption;
            bufferPools = new ConcurrentDictionary<string, Lazy<IBufferPool>>();

            var jobScheduler = new HerculesRecordsSendingJobScheduler(
                memoryManager,
                settings.RequestSendPeriod,
                settings.RequestSendPeriodCap);

            var requestSender = new RequestSender(settings.Cluster, log, settings.ApiKeyProvider, settings.ClusterClientSetup);

            var batcher = new BufferSnapshotBatcher(maximumBatchSize);

            var contentFactory = new RequestContentFactory();

            var job = new HerculesRecordsSendingJob(
                this,
                bufferPools,
                jobScheduler,
                batcher,
                contentFactory,
                requestSender,
                log,
                settings.RequestTimeout);

            recordsSendingDaemon = new HerculesRecordsSendingDaemon(log, job);
        }

        /// <summary>
        /// How many records are lost due to memory limit violation and network communication errors.
        /// </summary>
        public long LostRecordsCount => Interlocked.Read(ref lostRecordsCounter) + recordsSendingDaemon.LostRecordsCount;

        /// <summary>
        /// How many records already sent with this <see cref="HerculesSink"/>.
        /// </summary>
        public long SentRecordsCount =>
            recordsSendingDaemon.SentRecordsCount;

        /// <summary>
        /// How many records stored inside <see cref="HerculesSink"/> internal buffers and waiting to be sent.
        /// </summary>
        public long StoredRecordsCount =>
            bufferPools
                .Where(x => x.Value.IsValueCreated)
                .Select(x => x.Value.Value)
                .Sum(x => x.GetStoredRecordsCount());

        /// <inheritdoc />
        public void Put(string stream, Action<IHerculesEventBuilder> build)
        {
            if (!ValidateParameters(stream, build))
                return;

            var bufferPool = GetOrCreate(stream);

            if (!bufferPool.TryAcquire(out var buffer))
            {
                Interlocked.Increment(ref lostRecordsCounter);
                return;
            }

            try
            {
                var binaryWriter = buffer.BeginRecord();

                if (recordWriter.TryWrite(binaryWriter, build, out var recordSize))
                    buffer.Commit(recordSize);
                else
                    Interlocked.Increment(ref lostRecordsCounter);
            }
            finally
            {
                bufferPool.Release(buffer);
            }

            recordsSendingDaemon.Initialize();
        }

        /// <inheritdoc />
        public void ConfigureStream(string stream, StreamSettings settings)
        {
            var bufferPool = GetOrCreate(stream);
            bufferPool.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (Interlocked.CompareExchange(ref isDisposed, 1, 0) == 0)
                recordsSendingDaemon.Dispose();
        }

        private bool ValidateParameters(string stream, Action<IHerculesEventBuilder> build)
        {
            if (IsDisposed())
            {
                log.Warn("An attempt to put event to disposed HerculesSink.");
                return false;
            }

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

        private IBufferPool GetOrCreate(string stream) =>
            bufferPools.GetOrAdd(
                    stream,
                    _ => new Lazy<IBufferPool>(CreateBufferPool))
                .Value;

        private IBufferPool CreateBufferPool()
        {
            var perStreamMemoryManager = new MemoryManager(maximumPerStreamMemoryConsumptionBytes, memoryManager);

            return new BufferPool(
                perStreamMemoryManager,
                initialPooledBufferSize,
                maximumRecordSize,
                maximumBatchSize);
        }

        private bool IsDisposed()
        {
            return isDisposed == 1;
        }
    }
}