using JetBrains.Annotations;
using Vostok.Hercules.Client.Abstractions.Results;

namespace Vostok.Hercules.Client.Internal
{
    internal class RawReadStreamResult : HerculesResult<RawReadStreamPayload>
    {
        public RawReadStreamResult(HerculesStatus status, RawReadStreamPayload payload, [CanBeNull] string errorDetails = null)
            : base(status, payload, errorDetails)
        {
        }
    }
}