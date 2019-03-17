using Vostok.Hercules.Client.Abstractions.Results;

namespace Vostok.Hercules.Client.Sink.Analyzer
{
    internal class StatusAnalyzer : IStatusAnalyzer
    {
        public bool ShouldDropStoredRecords(HerculesStatus status)
        {
            switch (status)
            {
                case HerculesStatus.Success:
                case HerculesStatus.IncorrectRequest:
                case HerculesStatus.RequestTooLarge:
                    return true;
            }

            return false;
        }

        public bool ShouldIncreaseSendPeriod(HerculesStatus status)
        {
            switch (status)
            {
                case HerculesStatus.StreamNotFound:
                case HerculesStatus.Unauthorized:
                case HerculesStatus.InsufficientPermissions:
                case HerculesStatus.Throttled:
                case HerculesStatus.Timeout:
                case HerculesStatus.NetworkError:
                case HerculesStatus.ServerError:
                case HerculesStatus.UnknownError:
                    return true;
            }

            return false;
        }
    }
}