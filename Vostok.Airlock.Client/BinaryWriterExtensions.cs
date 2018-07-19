using System;
using System.Collections.Generic;
using Vostok.Commons.Binary;

namespace Vostok.Airlock.Client
{
    internal static class BinaryWriterExtensions
    {
        internal static IBinaryWriter WriteVector<T>(this IBinaryWriter writer, IReadOnlyCollection<T> values, Action<IBinaryWriter, T> writeSingleValue)
        {
            if (values == null)
                throw new ArgumentNullException(nameof(values));

            writer.Write((byte)values.Count);

            foreach (var value in values)
                writeSingleValue(writer, value);

            return writer;
        }
    }
}