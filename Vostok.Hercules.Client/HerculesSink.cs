using System;
using System.Collections.Concurrent;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Commons.Threading;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Abstractions.Models;
using Vostok.Hercules.Client.Sink.Daemon;
using Vostok.Hercules.Client.Sink.Job;
using Vostok.Hercules.Client.Sink.Scheduler;
using Vostok.Hercules.Client.Sink.Scheduler.Helpers;
using Vostok.Hercules.Client.Sink.State;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client
{
    /// <inheritdoc cref="IHerculesSink" />
    [PublicAPI]
    public class HerculesSink : IHerculesSink, IDisposable
    {
        private readonly HerculesSinkSettings settings;
        private readonly IStreamStateFactory stateFactory;
        private readonly IDaemon sendingDaemon;
        private readonly ILog log;

        private readonly ConcurrentDictionary<string, Lazy<IStreamState>> states;
        private readonly AtomicBoolean isDisposed;

        public HerculesSink([NotNull] HerculesSinkSettings settings, [CanBeNull] ILog log)
        {
            this.settings = settings ?? throw new ArgumentNullException(nameof(settings));
            this.log = (log ?? LogProvider.Get()).ForContext<HerculesSink>();

            states = new ConcurrentDictionary<string, Lazy<IStreamState>>();
            isDisposed = new AtomicBoolean(false);

            var jobLauncher = new JobLauncher();
            var jobHandler = new JobHandler(jobLauncher);
            var jobWaiter = new JobWaiter(settings.SendPeriod, settings.MaxParallelStreams);
            var jobFactory = new StreamJobFactory(settings, this.log);
            var statesProvider = new StreamStatesProvider(states);
            var stateSynchronizer = new StateSynchronizer(statesProvider, jobFactory, jobLauncher);
            var flowController = new FlowController(new WeakReference(this));
            var scheduler = new Scheduler(stateSynchronizer, flowController, jobWaiter, jobHandler);

            stateFactory = new StreamStateFactory(settings, this.log);
            sendingDaemon = new Daemon(scheduler);
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
                streamState.RecordWriter.TryWrite(buffer, build, out _);
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
            var perStreamCounters = states
                .Where(x => x.Value.IsValueCreated)
                .ToDictionary(x => x.Key, x => x.Value.Value.Statistics.GetCounters());

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
            states
                .GetOrAdd(stream, s => new Lazy<IStreamState>(() => stateFactory.Create(s)))
                .Value;
    }
}
