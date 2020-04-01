using System;
using Kontur.Lz4;
using Vostok.Logging.Abstractions;

namespace Vostok.Hercules.Client.Internal
{
    // ReSharper disable once InconsistentNaming
    internal static class LZ4Helper
    {
        public static bool Enabled(ILog log)
        {
            try
            {
                LZ4Codec.CompressBound(42);
                return true;
            }
            catch (Exception e)
            {
                log.Warn(e, "LZ4 compression compression disabled due to error.");
                return false;
            }
        }
    }
}