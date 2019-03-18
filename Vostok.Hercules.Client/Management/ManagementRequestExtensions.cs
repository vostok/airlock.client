using JetBrains.Annotations;
using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hercules.Client.Management
{
    internal static class ManagementRequestExtensions
    {
        [NotNull]
        public static Request WithStreamName([NotNull] this Request request, [NotNull] string stream)
            => request.WithAdditionalQueryParameter(Constants.QueryParameters.Stream, stream);

        [NotNull]
        public static Request WithTimelineName([NotNull] this Request request, [NotNull] string timeline)
            => request.WithAdditionalQueryParameter(Constants.QueryParameters.Timeline, timeline);
    }
}