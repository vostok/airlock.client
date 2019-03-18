using System.IO;
using JetBrains.Annotations;

namespace Vostok.Hercules.Client.Serialization.Json
{
    internal interface IJsonSerializer
    {
        byte[] Serialize([NotNull] object item);
        T Deserialize<T>(Stream stream);
    }
}