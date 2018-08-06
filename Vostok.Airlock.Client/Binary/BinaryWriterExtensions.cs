using System;
using System.Collections.Generic;
using System.Text;

namespace Vostok.Airlock.Client.Binary
{
    internal static class BinaryWriterExtensions
    {
        public static void WriteCollection<T>(
            this IBinaryWriter writer,
            IReadOnlyCollection<T> values,
            Action<IBinaryWriter, T> writeSingleValue)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            writer.Write(values.Count);

            foreach (var value in values)
                writeSingleValue(writer, value);
        }

        public static void WriteVector<T>(
            this IBinaryWriter writer,
            IReadOnlyCollection<T> values,
            Action<IBinaryWriter, T> writeSingleValue)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            writer.Write((byte)values.Count);

            foreach (var value in values)
                writeSingleValue(writer, value);
        }

        public static IBinaryWriter Write(this IBinaryWriter writer, string value)
        {
            writer.Write(value, Encoding.UTF8);
            return writer;
        }

        public static IBinaryWriter WriteWithoutLengthPrefix(this IBinaryWriter writer, string value)
        {
            writer.WriteWithoutLengthPrefix(value, Encoding.UTF8);
            return writer;
        }
    }
}