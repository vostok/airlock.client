using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Sink.State
{
    internal interface IStreamStatesProvider
    {
        [NotNull]
        [ItemNotNull]
        IEnumerable<IStreamState> GetStates();
    }
}