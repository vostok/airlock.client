using System;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.Planning;
using Vostok.Hercules.Client.Sink.Sender;
using Vostok.Hercules.Client.Sink.State;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Job
{
    internal class StreamJobFactory : IStreamJobFactory
    {
        private readonly IStreamSenderFactory senderFactory;
        private readonly IPlannerFactory plannerFactory;
        private readonly ILog log;

        private readonly TimeSpan requestTimeout;

        public StreamJobFactory([NotNull] HerculesSinkSettings settings, [NotNull] ILog log)
        {
            this.log = log;

            senderFactory = new StreamSenderFactory(settings, log);
            plannerFactory = new PlannerFactory(settings);
            requestTimeout = settings.RequestTimeout;
        }

        public IStreamJob CreateJob(IStreamState state) 
            => new StreamJob(senderFactory.Create(state), plannerFactory.Create(state), log.ForContext(state.Name), requestTimeout);
    }
}