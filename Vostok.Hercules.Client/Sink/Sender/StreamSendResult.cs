using System;
using Vostok.Hercules.Client.Abstractions.Results;

namespace Vostok.Hercules.Client.Sink.Sender
{
    internal class StreamSendResult
    {
        public static readonly StreamSendResult Success
            = new StreamSendResult(HerculesStatus.Success, TimeSpan.Zero);

        public StreamSendResult(HerculesStatus status, TimeSpan elapsed)
        {
            Status = status;
            Elapsed = elapsed;
        }

        public HerculesStatus Status { get; }

        public TimeSpan Elapsed { get; }
    }
}
