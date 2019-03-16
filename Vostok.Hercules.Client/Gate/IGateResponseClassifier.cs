using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hercules.Client.Gate
{
    internal interface IGateResponseClassifier
    {
        GateResponseClass Classify([NotNull] Response response);
    }
}