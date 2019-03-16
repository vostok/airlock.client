using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Hercules.Client.Sink.Planner;
using Vostok.Hercules.Client.Sink.Sender;

namespace Vostok.Hercules.Client.Sink.Job
{
    internal class StreamJob : IStreamJob
    {
        private readonly IStreamSender sender;
        private readonly IPlanner planner;
        private readonly TimeSpan requestTimeout;

        public StreamJob(IStreamSender sender, IPlanner planner, TimeSpan requestTimeout)
        {
            this.sender = sender;
            this.planner = planner;
            this.requestTimeout = requestTimeout;
        }

        public Task SendAsync(CancellationToken token) =>
            throw new System.NotImplementedException();

        public Task WaitForNextSendAsync(CancellationToken token) =>
            throw new System.NotImplementedException();
    }
}