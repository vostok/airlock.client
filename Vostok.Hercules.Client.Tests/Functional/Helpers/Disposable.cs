using System;

namespace Vostok.Hercules.Client.Tests.Functional.Helpers
{
    internal class Disposable : IDisposable
    {
        private readonly Action onDispose;

        public Disposable(Action onDispose) => this.onDispose = onDispose;

        public void Dispose() => onDispose();
    }
}