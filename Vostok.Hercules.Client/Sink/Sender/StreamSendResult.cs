using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Vostok.Hercules.Client.Gate;

namespace Vostok.Hercules.Client.Sink.Sender
{
    internal class StreamSendResult
    {
        public static readonly StreamSendResult Empty
            = new StreamSendResult(new GateResponseClass[] {}, TimeSpan.Zero);

        public StreamSendResult([NotNull] IReadOnlyList<GateResponseClass> batchResults, TimeSpan elapsed)
        {
            BatchResults = batchResults;
            Elapsed = elapsed;
        }

        /// <summary>
        /// A list of response classes, one for each batch sent.
        /// </summary>
        [NotNull]
        public IReadOnlyList<GateResponseClass> BatchResults { get; }

        /// <summary>
        /// Returns the time it took to send stream data;
        /// </summary>
        public TimeSpan Elapsed { get; }

        /// <summary>
        /// Returns <c>true</c> if any of <see cref="BatchResults"/> is <see cref="GateResponseClass.TransientFailure"/>.
        /// </summary>
        public bool HasTransientFailures => BatchResults.Contains(GateResponseClass.TransientFailure);

        /// <summary>
        /// Returns <c>true</c> if all <see cref="BatchResults"/> are successful.
        /// </summary>
        public bool IsSuccessful => BatchResults.All(result => result == GateResponseClass.Success);
    }
}
