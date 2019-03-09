using System;

namespace Vostok.Hercules.Client.Sink.Daemon
{
    internal interface IRecordsSendingDaemon : IDisposable
    {
        void Initialize();
    }
}