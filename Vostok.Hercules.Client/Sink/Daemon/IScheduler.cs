using System;
using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Daemon
{
    internal interface IScheduler : IDisposable
    {
        Task RunAsync(CancellationToken cancellationToken);
    }
}