namespace Vostok.Hercules.Client.Gate
{
    internal enum GateResponseClass
    {
        /// <summary>
        /// Records have been successfully sent.
        /// </summary>
        Success,

        /// <summary>
        /// A failure has occurred that's likely to resolve by itself.
        /// </summary>
        TransientFailure,

        /// <summary>
        /// A failure has occurred that's not likely to resolve by itself.
        /// </summary>
        DefinitiveFailure
    }
}