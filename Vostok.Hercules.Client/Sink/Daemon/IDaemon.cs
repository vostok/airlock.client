using System;

namespace Vostok.Hercules.Client.Sink.Daemon
{
    internal interface IDaemon : IDisposable
    {
        void Initialize();
    }
}