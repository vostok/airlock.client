using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Hercules.Client.Sink.Planning;
using Vostok.Hercules.Client.Sink.Sender;

namespace Vostok.Hercules.Client.Sink.Job
{
    internal class StreamJob : IStreamJob
    {
        private readonly IStreamSender sender;
        private readonly IPlanner planner;
        private readonly TimeSpan requestTimeout;

        private volatile StreamSendResult lastSendResult = StreamSendResult.Success;

        public StreamJob(IStreamSender sender, IPlanner planner, TimeSpan requestTimeout)
        {
            this.sender = sender;
            this.planner = planner;
            this.requestTimeout = requestTimeout;
        }

        public async Task SendAsync(CancellationToken cancellationToken) 
            => lastSendResult = await sender.SendAsync(requestTimeout, cancellationToken).ConfigureAwait(false);

        public Task WaitForNextSendAsync(CancellationToken cancellationToken) 
            => planner.WaitForNextSendAsync(lastSendResult, cancellationToken);
    }
}