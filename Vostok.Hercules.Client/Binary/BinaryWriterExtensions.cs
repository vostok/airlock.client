using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Vostok.Hercules.Client.Binary
{
    internal static class BinaryWriterExtensions
    {
        public static IBinaryWriter WriteCollection<T>(
            this IBinaryWriter writer,
            IReadOnlyCollection<T> value,
            Action<IBinaryWriter, T> writeSingleValue)
        {
            writer.WriteInNetworkByteOrder(value.Count);

            foreach (var singleValue in value)
                writeSingleValue(writer, singleValue);

            return writer;
        }

        public static IBinaryWriter WriteVector<T>(
            this IBinaryWriter writer,
            IReadOnlyCollection<T> value,
            Action<IBinaryWriter, T> writeSingleValue)
        {
            writer.Write((byte) value.Count);

            foreach (var singleValue in value)
                writeSingleValue(writer, singleValue);

            return writer;
        }

        public static IBinaryWriter WriteWithInt32LengthPrefix(this IBinaryWriter writer, string value) =>
            writer.WriteInNetworkByteOrder(Encoding.UTF8.GetByteCount(value))
                .Write(value, Encoding.UTF8);

        public static IBinaryWriter WriteWithByteLengthPrefix(this IBinaryWriter writer, string value) =>
            writer.Write((byte) Encoding.UTF8.GetByteCount(value))
                .Write(value, Encoding.UTF8);

        public static IBinaryWriter WriteWithInt32LengthPrefix(this IBinaryWriter writer, byte[] value) =>
            writer.WriteInNetworkByteOrder(value.Length)
                .Write(value, 0, value.Length);

        public static IBinaryWriter WriteWithoutLengthPrefix(this IBinaryWriter writer, byte[] value) =>
            writer.Write(value, 0, value.Length);

        public static IBinaryWriter WriteWithByteLengthPrefix(this IBinaryWriter writer, byte[] value) =>
            writer.Write((byte) value.Length)
                .Write(value, 0, value.Length);

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, int value) =>
            writer.Write(IPAddress.HostToNetworkOrder(value));

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, long value) =>
            writer.Write(IPAddress.HostToNetworkOrder(value));

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, short value) =>
            writer.Write(IPAddress.HostToNetworkOrder(value));

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, double value) =>
            writer.Write(BitConverter.Int64BitsToDouble(IPAddress.HostToNetworkOrder(BitConverter.DoubleToInt64Bits(value))));

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, float value) =>
            writer.Write(Int32BitsToSingle(IPAddress.HostToNetworkOrder(SingleToInt32Bits(value))));

        private static unsafe int SingleToInt32Bits(float value) => *((int*) &value);

        private static unsafe float Int32BitsToSingle(int value) => *((float*) &value);
    }
}