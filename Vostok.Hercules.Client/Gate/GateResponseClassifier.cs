using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Abstractions.Results;
using Vostok.Hercules.Client.Client;

namespace Vostok.Hercules.Client.Gate
{
    internal class GateResponseClassifier : IGateResponseClassifier
    {
        private readonly IResponseAnalyzer responseAnalyzer;

        public GateResponseClassifier(IResponseAnalyzer responseAnalyzer)
        {
            this.responseAnalyzer = responseAnalyzer;
        }

        public GateResponseClass Classify(Response response)
        {
            var status = responseAnalyzer.Analyze(response, out _);

            switch (status)
            {
                case HerculesStatus.Success:
                    return GateResponseClass.Success;

                case HerculesStatus.Throttled:
                case HerculesStatus.Canceled:
                case HerculesStatus.Timeout:
                case HerculesStatus.NetworkError:
                case HerculesStatus.ServerError:
                    return GateResponseClass.TransientFailure;

                default:
                    return GateResponseClass.DefinitiveFailure;
            }
        }
    }
}