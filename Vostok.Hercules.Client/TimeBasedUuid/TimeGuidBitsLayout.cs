using System;

namespace Vostok.Hercules.Client.TimeBasedUuid
{
    // Version 1 UUID layout (https://www.ietf.org/rfc/rfc4122.txt):
    //
    // Most significant long:
    // 0xFFFFFFFF00000000 time_low
    // 0x00000000FFFF0000 time_mid
    // 0x000000000000F000 version
    // 0x0000000000000FFF time_hi
    //
    // Least significant long:
    // 0xC000000000000000 variant
    // 0x3FFF000000000000 clock_sequence
    // 0x0000FFFFFFFFFFFF node
    //
    // Or in more detail from most significant to least significant byte (octet):
    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    // |0               1               2               3              |
    // |7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|7 6 5 4 3 2 1 0|
    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    // |(msb)                      time_low                            |
    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    // |           time_mid            |  ver  |       time_hi         |
    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    // |var| clkseq_hi |  clkseq_low   |           node(0-1)           |
    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    // |                           node(2-5)                      (lsb)|
    // +-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+-+
    //
    // Implementation is based on https://github.com/fluentcassandra/fluentcassandra/blob/master/src/GuidGenerator.cs
    internal static class TimeGuidBitsLayout
    {
        public const int NodeSize = 6;

        public const ushort MinClockSequence = 0;
        public const ushort MaxClockSequence = 16383; /* = 0x3fff */

        private const int SignBitMask = 0x80;

        private const int VersionOffset = 6;
        private const byte VersionByteMask = 0x0f;
        private const int VersionByteShift = 4;

        private const int VariantOffset = 8;
        private const byte VariantByteMask = 0x3f;
        private const byte VariantBitsValue = 0x80;

        // min timestamp representable by time-based UUID is gregorian calendar 0-time (1582-10-15 00:00:00Z)
        private static readonly long gregorianCalendarStart = new DateTime(1582, 10, 15, 0, 0, 0, DateTimeKind.Utc).Ticks;

        // max timestamp representable by time-based UUID (~5236-03-31 21:21:00Z)
        private static readonly long gregorianCalendarEnd = new DateTime(1652084544606846975L, DateTimeKind.Utc).Ticks;

        public static byte[] Format(long timestamp, ushort clockSequence, byte[] node)
        {
            if (node.Length != NodeSize)
                throw new ArgumentException($"Node must be {NodeSize} bytes long", nameof(node));
            if (timestamp < gregorianCalendarStart)
                throw new ArgumentException($"Timestamp must not be less than {gregorianCalendarStart}", nameof(timestamp));
            if (timestamp > gregorianCalendarEnd)
                throw new ArgumentException($"Timestamp must not be greater than {gregorianCalendarEnd}", nameof(timestamp));
            if (clockSequence > MaxClockSequence)
                throw new ArgumentException($"ClockSequence must not be greater than {MaxClockSequence}", nameof(clockSequence));

            var timestampTicks = timestamp - gregorianCalendarStart;

            var timestampBytes = BitConverter.GetBytes(timestampTicks);
            if (!BitConverter.IsLittleEndian)
            {
                Array.Reverse(timestampBytes);
            }

            var clockSequencebytes = BitConverter.GetBytes(clockSequence);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(clockSequencebytes);
            }
            
            var bytes = new byte[TimeGuid.Size];
            bytes[0] = timestampBytes[3];
            bytes[1] = timestampBytes[2];
            bytes[2] = timestampBytes[1];
            bytes[3] = timestampBytes[0];
            bytes[4] = timestampBytes[5];
            bytes[5] = timestampBytes[4];
            bytes[6] = timestampBytes[7];
            bytes[7] = timestampBytes[6];

            // xor octets 8-15 with 10000000 for cassandra compatibility as it compares these octets as signed bytes
            var offset = 8;
            for (var i = 0; i < sizeof(ushort); i++)
                bytes[offset++] = (byte) (clockSequencebytes[i] ^ SignBitMask);
            for (var i = 0; i < NodeSize; i++)
                bytes[offset++] = (byte) (node[i] ^ SignBitMask);

            // octets[ver_and_timestamp_hi] := 0001xxxx
            bytes[VersionOffset] &= VersionByteMask;
            bytes[VersionOffset] |= (byte) GuidVersion.TimeBased << VersionByteShift;

            // octets[variant_and_clock_sequence] := 10xxxxxx
            bytes[VariantOffset] &= VariantByteMask;
            bytes[VariantOffset] |= VariantBitsValue;

            return bytes;
        }
    }
}