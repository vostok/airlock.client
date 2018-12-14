using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using Vostok.Commons.Binary;

namespace Vostok.Hercules.Client.Binary
{
    internal static class HerculesBinaryWriterExtensions
    {
        public static void Write(this IHerculesBinaryWriter writer, TagValueTypeDefinition valueType) =>
            writer.Write((byte) valueType);
        
        public static void WriteCollection<T>(
            [NotNull] this IHerculesBinaryWriter writer, 
            [NotNull] IReadOnlyCollection<T> items,
            [NotNull] Action<IHerculesBinaryWriter, T> writeSingleValue)
        {
            writer.Write(items.Count);

            foreach (var item in items)
            {
                writeSingleValue(writer, item);
            }
        }
        
        public static void WriteWithByteLength(this IHerculesBinaryWriter writer, string value)
        {
            var lengthPosition = writer.Position;
            writer.Write(0b0);
            var startPosition = writer.Position;
            writer.WriteWithoutLength(value);
            var positionAfter = writer.Position;

            var length = positionAfter - startPosition;

            if (length > 255)
                writer.IsOverflowed = true;
            
            writer.Position = lengthPosition;
            writer.Write((byte) length);
            
            writer.Position = positionAfter;
        }
    }
}