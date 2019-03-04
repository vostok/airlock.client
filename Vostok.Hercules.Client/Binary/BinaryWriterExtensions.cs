using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using JetBrains.Annotations;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client.Binary
{
    internal static class BinaryWriterExtensions
    {
        public static void Write(this IBinaryWriter writer, TagType valueType) =>
            writer.Write((byte)valueType);

        public static void WriteReadOnlyCollection<T>(
            [NotNull] this IBinaryWriter writer,
            [NotNull] IReadOnlyCollection<T> items,
            [NotNull] Action<IBinaryWriter, T> writeSingleValue)
        {
            writer.Write(items.Count);

            foreach (var item in items)
            {
                writeSingleValue(writer, item);
            }
        }

        public static void WriteWithByteLength(this IBinaryWriter writer, string value)
        {
            const int maxLength = byte.MaxValue;

            var lengthPosition = writer.Position;
            writer.Write((byte)0);
            
            var startPosition = writer.Position;
            writer.WriteWithoutLength(value);
            var positionAfter = writer.Position;

            var length = positionAfter - startPosition;

            if (length > maxLength)
                throw new ArgumentOutOfRangeException(nameof(value), $"String value '{value}' doesn't fit in {maxLength} bytes in UTF-8 encoding.");

            using (writer.JumpTo(lengthPosition))
                writer.Write((byte)length);
        }
    }
}