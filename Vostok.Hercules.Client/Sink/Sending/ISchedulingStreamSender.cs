using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Sink.Sending
{
    internal interface ISchedulingStreamSender : IStreamSender
    {
        Task Signal { get; }
        void Wakeup();
    }
}