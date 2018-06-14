using System.Threading.Tasks;

namespace Vostok.Airlock.Client
{
    internal static class TaskExtensions
    {
        public static Task SilentlyContinue(this Task source)
        {
            return source.ContinueWith(_ => { });
        }
    }
}