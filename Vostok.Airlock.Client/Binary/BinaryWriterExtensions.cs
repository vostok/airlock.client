using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Vostok.Airlock.Client.Binary
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
            writer.Write((byte)value.Count);

            foreach (var singleValue in value)
                writeSingleValue(writer, singleValue);

            return writer;
        }

        public static IBinaryWriter WriteWithInt32LengthPrefix(this IBinaryWriter writer, string value)
        {
            return writer.WriteInNetworkByteOrder(Encoding.UTF8.GetByteCount(value))
                         .Write(value, Encoding.UTF8);
        }

        public static IBinaryWriter WriteWithByteLengthPrefix(this IBinaryWriter writer, string value)
        {
            return writer.Write((byte)Encoding.UTF8.GetByteCount(value))
                         .Write(value, Encoding.UTF8);
        }

        public static IBinaryWriter WriteWithInt32LengthPrefix(this IBinaryWriter writer, byte[] value)
        {
            return writer.WriteInNetworkByteOrder(value.Length)
                         .Write(value, 0, value.Length);
        }

        public static IBinaryWriter WriteWithoutLengthPrefix(this IBinaryWriter writer, byte[] value)
        {
            return writer.Write(value, 0, value.Length);
        }

        public static IBinaryWriter WriteWithByteLengthPrefix(this IBinaryWriter writer, byte[] value)
        {
            return writer.Write((byte)value.Length)
                         .Write(value, 0, value.Length);
        }

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, int value)
        {
            return writer.Write(IPAddress.HostToNetworkOrder(value));
        }

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, long value)
        {
            return writer.Write(IPAddress.HostToNetworkOrder(value));
        }

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, short value)
        {
            return writer.Write(IPAddress.HostToNetworkOrder(value));
        }

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, double value)
        {
            return writer.Write(BitConverter.Int64BitsToDouble(IPAddress.HostToNetworkOrder(BitConverter.DoubleToInt64Bits(value))));
        }

        public static IBinaryWriter WriteInNetworkByteOrder(this IBinaryWriter writer, float value)
        {
            return writer.Write(Int32BitsToSingle(IPAddress.HostToNetworkOrder(SingleToInt32Bits(value))));
        }

        private static unsafe int SingleToInt32Bits(float value) => *((int*)&value);

        private static unsafe float Int32BitsToSingle(int value) => *((float*)&value);
    }
}