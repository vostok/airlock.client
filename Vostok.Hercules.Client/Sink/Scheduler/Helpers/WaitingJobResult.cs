using JetBrains.Annotations;
using Vostok.Hercules.Client.Sink.Job;

namespace Vostok.Hercules.Client.Sink.Scheduler.Helpers
{
    internal class WaitingJobResult
    {
        public WaitingJobResult([NotNull] IStreamJob job)
            => Job = job;

        [NotNull]
        public IStreamJob Job { get; }
    }
}