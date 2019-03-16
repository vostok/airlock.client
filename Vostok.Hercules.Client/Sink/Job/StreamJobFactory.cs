using Vostok.Hercules.Client.Sink.State;

namespace Vostok.Hercules.Client.Sink.Job
{
    internal class StreamJobFactory : IStreamJobFactory
    {
        public IStreamJob CreateJob(IStreamState state) =>
            throw new System.NotImplementedException();
    }
}