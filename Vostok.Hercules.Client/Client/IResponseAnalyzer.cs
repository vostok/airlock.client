using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Abstractions.Results;

namespace Vostok.Hercules.Client.Client
{
    internal interface IResponseAnalyzer
    {
        HerculesStatus Analyze([NotNull] Response response, out string errorMessage);
    }
}