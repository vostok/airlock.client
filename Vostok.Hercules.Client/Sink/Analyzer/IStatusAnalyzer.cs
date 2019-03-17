using Vostok.Hercules.Client.Abstractions.Results;

namespace Vostok.Hercules.Client.Sink.Analyzer
{
    internal interface IStatusAnalyzer
    {
        bool ShouldDropStoredRecords(HerculesStatus status);

        bool ShouldIncreaseSendPeriod(HerculesStatus status);
    }
}
