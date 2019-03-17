using Vostok.Hercules.Client.Sink.Analyzer;
using Vostok.Hercules.Client.Sink.State;

namespace Vostok.Hercules.Client.Sink.Planning
{
    internal class PlannerFactory : IPlannerFactory
    {
        private const double MaxJitterFraction = 0.2;

        private readonly HerculesSinkSettings settings;

        public PlannerFactory(HerculesSinkSettings settings) =>
            this.settings = settings;

        public IPlanner Create(IStreamState state) =>
            new Planner(new StatusAnalyzer(), state.SendSignal, settings.SendPeriod, settings.SendPeriodCap, MaxJitterFraction);
    }
}