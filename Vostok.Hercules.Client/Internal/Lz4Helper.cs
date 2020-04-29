using System;
using System.Collections;
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
                var random = new Random(42);
                
                var source = new byte[42];
                random.NextBytes(source);

                var compressed = new byte[LZ4Codec.MaximumOutputSize(source.Length)];
                var compressedLength = LZ4Codec.Encode(source, 0, source.Length, compressed, 0, compressed.Length);

                var decompressed = new byte[source.Length];
                LZ4Codec.Decode(compressed, 0, compressedLength, decompressed, 0, decompressed.Length);

                if (!StructuralComparisons.StructuralEqualityComparer.Equals(source, decompressed))
                    throw new Exception("Decompressed bytes not equal to source bytes.");

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