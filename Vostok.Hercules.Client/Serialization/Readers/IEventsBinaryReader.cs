using System.Collections.Generic;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Serialization.Readers
{
    [PublicAPI]
    public interface IEventsBinaryReader<T>
    {
        IList<T> Read(byte[] bytes, int offset);
    }
}