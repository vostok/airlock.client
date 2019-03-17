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
        private readonly State state;

        public HerculesSink([NotNull] HerculesSinkSettings settings, [CanBeNull] ILog log)
        {
            state = ConstructInternalState(this, settings, log);
        }

        internal HerculesSink(IStreamStateFactory streamStateFactory, IDaemon daemon, ILog log)
        {
            state = new State(streamStateFactory, daemon, log, new ConcurrentDictionary<string, Lazy<IStreamState>>());
        }

        /// <inheritdoc />
        public void Put(string streamName, Action<IHerculesEventBuilder> build)
        {
            if (!CanPut(streamName, build))
                return;

            var stream = ObtainStream(streamName);

            if (!stream.BufferPool.TryAcquire(out var buffer))
            {
                stream.Statistics.ReportOverflow();

                if (stream.Statistics.EstimateStoredSize() > 0)
                    stream.SendSignal.Set();

                return;
            }

            try
            {
                stream.RecordWriter.TryWrite(buffer, build, out _);
            }
            finally
            {
                stream.BufferPool.Release(buffer);
            }

            state.Daemon.Initialize();
        }

        /// <inheritdoc />
        public void ConfigureStream(string stream, StreamSettings streamSettings)
            => ObtainStream(stream).Settings = streamSettings ?? throw new ArgumentNullException(nameof(streamSettings));

        /// <summary>
        /// <para>Provides diagnostics information about <see cref="HerculesSink"/>.</para>
        /// </summary>
        public HerculesSinkStatistics GetStatistics()
        {
            var perStreamCounters = new StreamStatesProvider(state.Streams)
                .GetStates()
                .ToDictionary(s => s.Name, s => s.Statistics.GetCounters());

            var totalCounters = HerculesSinkCounters.Zero;

            foreach (var value in perStreamCounters.Values)
                totalCounters += value;

            return new HerculesSinkStatistics(totalCounters, perStreamCounters);
        }

        /// <inheritdoc />
        public void Dispose()
        {
            if (state.IsDisposed.TrySetTrue())
                state.Daemon.Dispose();
        }

        private static State ConstructInternalState(
            [NotNull] HerculesSink sink,
            [NotNull] HerculesSinkSettings settings,
            [CanBeNull] ILog log)
        {
            log = (log ?? LogProvider.Get()).ForContext<HerculesSink>();

            var jobLauncher = new JobLauncher();
            var jobHandler = new JobHandler(jobLauncher);
            var jobWaiter = new JobWaiter(settings.SendPeriod, settings.MaxParallelStreams);
            var jobFactory = new StreamJobFactory(settings, log);
            var streamStates = new ConcurrentDictionary<string, Lazy<IStreamState>>();
            var streamStatesFactory = new StreamStateFactory(settings, log);
            var streamStatesProvider = new StreamStatesProvider(streamStates);
            var stateSynchronizer = new StateSynchronizer(streamStatesProvider, jobFactory, jobLauncher);
            var flowController = new FlowController(new WeakReference(sink));
            var scheduler = new Scheduler(stateSynchronizer, flowController, jobWaiter, jobHandler);
            var daemon = new Daemon(scheduler);

            return new State(streamStatesFactory, daemon, log, streamStates);
        }

        private IStreamState ObtainStream(string name)
            => state.Streams.GetOrAdd(name, v => new Lazy<IStreamState>(() => state.Factory.Create(v))).Value;

        private bool CanPut(string streamName, Action<IHerculesEventBuilder> build)
        {
            if (state.IsDisposed)
            {
                LogPutOnDisposedSink();
                return false;
            }

            if (string.IsNullOrEmpty(streamName))
            {
                LogIncorrectStreamName();
                return false;
            }

            if (build == null)
            {
                LogNullBuildDelegate();
                return false;
            }

            return true;
        }

        #region Internal state class

        private class State
        {
            public State(
                [NotNull] IStreamStateFactory factory,
                [NotNull] IDaemon daemon,
                [NotNull] ILog log,
                [NotNull] ConcurrentDictionary<string, Lazy<IStreamState>> streams)
            {
                Factory = factory ?? throw new ArgumentNullException(nameof(factory));
                Streams = streams ?? throw new ArgumentNullException(nameof(streams));
                Daemon = daemon ?? throw new ArgumentNullException(nameof(daemon));
                Log = log.ForContext<HerculesSink>();
            }

            [NotNull]
            public AtomicBoolean IsDisposed { get; } = new AtomicBoolean(false);

            [NotNull]
            public ConcurrentDictionary<string, Lazy<IStreamState>> Streams { get; }

            [NotNull]
            public IStreamStateFactory Factory { get; }

            [NotNull]
            public IDaemon Daemon { get; }

            [NotNull]
            public ILog Log { get; }
        }

        #endregion

        #region Logging

        private void LogPutOnDisposedSink()
            => state.Log.Warn($"An attempt to put event to a disposed {nameof(HerculesSink)}.");

        private void LogIncorrectStreamName()
            => state.Log.Warn("An attempt to put event to a stream with a null or empty name.");

        private void LogNullBuildDelegate()
            => state.Log.Warn("User-provided event builder delegate was null.");

        #endregion
    }
}
