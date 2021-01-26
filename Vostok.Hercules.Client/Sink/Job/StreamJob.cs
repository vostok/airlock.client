using System;
using System.Threading;
using System.Threading.Tasks;
using Vostok.Commons.Helpers.Extensions;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Sink.Planning;
using Vostok.Hercules.Client.Sink.Sender;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Sink.Job
{
    internal class StreamJob : IStreamJob
    {
        private readonly IStreamSender sender;
        private readonly IPlanner planner;
        private readonly ILog log;
        private readonly TimeSpan requestTimeout;

        private volatile StreamSendResult lastSendResult = StreamSendResult.Success;

        public StreamJob(IStreamSender sender, IPlanner planner, ILog log, TimeSpan requestTimeout)
        {
            this.log = log;
            this.sender = sender;
            this.planner = planner;
            this.requestTimeout = requestTimeout;
        }

        public bool IsHealthy
        {
            get
            {
                var status = lastSendResult.Status;
                return status == HerculesStatus.Success || status == HerculesStatus.Canceled;
            }
        }

        public async Task SendAsync(CancellationToken cancellationToken)
        {
            try
            {
                lastSendResult = await sender.SendAsync(requestTimeout, cancellationToken).ConfigureAwait(false);
            }
            catch (Exception error)
            {
                if (!cancellationToken.IsCancellationRequested)
                    log.Error(error);
            }
        }

        public Task WaitForNextSendAsync(CancellationToken cancellationToken)
            => planner.WaitForNextSendAsync(lastSendResult, cancellationToken).SilentlyContinue();
    }
}