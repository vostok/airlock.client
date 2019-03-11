using System.Threading;
using System.Threading.Tasks;

namespace Vostok.Hercules.Client.Utilities
{
    internal static class CancellationTokenExtensions
    {
        public static CancellationTokenRegistration CreateTask(this CancellationToken cancellationToken, out Task task)
        {
            var tcs = new TaskCompletionSource<bool>();
            var registration = cancellationToken.Register(() => tcs.TrySetCanceled(cancellationToken));
            task = tcs.Task;
            return registration;
        }
    }
}