using System;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client.Serialization.Helpers
{
    internal static class BinaryReaderExtensions
    {
        public static IBinaryReader EnsureBigEndian(this IBinaryReader reader)
        {
            if (reader.Endianness != Endianness.Big)
                throw new ArgumentException("Provided binary reader is little endian.", nameof(reader));

            return reader;
        }
    }
}