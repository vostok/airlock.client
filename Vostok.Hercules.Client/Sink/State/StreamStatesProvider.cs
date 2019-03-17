using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Hercules.Client.Sink.State
{
    internal class StreamStatesProvider : IStreamStatesProvider
    {
        private readonly ConcurrentDictionary<string, Lazy<IStreamState>> states;

        public StreamStatesProvider(ConcurrentDictionary<string, Lazy<IStreamState>> states)
            => this.states = states;

        public IEnumerable<IStreamState> GetStates()
            => states.Where(pair => pair.Value.IsValueCreated).Select(pair => pair.Value.Value);
    }
}