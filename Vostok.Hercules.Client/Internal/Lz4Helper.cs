using System;
using K4os.Compression.LZ4;
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
                LZ4Codec.MaximumOutputSize(42);
                LogProvider.Get().ForContext("HerculesClient").Info("Lz4 compression enabled.");
                Enabled = true;
            }
            catch (Exception e)
            {
                LogProvider.Get().ForContext("HerculesClient").Warn(e, "Lz4 compression disabled due to error.");
                Enabled = false;
            }
        }
    }
}