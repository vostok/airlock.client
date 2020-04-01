using System;
using Kontur.Lz4;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Internal
{
    // ReSharper disable once InconsistentNaming
    internal static class LZ4Helper
    {
        public static readonly bool Enabled;

        static LZ4Helper()
        {
            try
            {
                LZ4Codec.CompressBound(42);
                Enabled = true;
            }
            catch (Exception e)
            {
                LogProvider.Get().ForContext("HerculesClient").Warn(e, "LZ4 compression disabled due to error.");
                Enabled = false;
            }
        }
    }
}