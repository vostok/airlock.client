using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Airlock.Client.Abstractions
{
    public static class AirlockRecordBuilderExtensions
    {
        public static IAirlockRecordBuilder Add(this IAirlockRecordBuilder source, Dictionary<string, TagValue> tags)
        {
            return tags.Aggregate(source, (builder, tag) => Add(builder, tag.Key, tag.Value));
        }

        public static IAirlockRecordBuilder Add(this IAirlockRecordBuilder source, string tagKey, TagValue tagValue)
        {
            switch (tagValue.Type)
            {
                case TagValueType.Byte:
                    return source.Add(tagKey, (byte) tagValue.Source);
                case TagValueType.Short:
                    return source.Add(tagKey, (short) tagValue.Source);
                case TagValueType.Int:
                    return source.Add(tagKey, (int) tagValue.Source);
                case TagValueType.Long:
                    return source.Add(tagKey, (long) tagValue.Source);
                case TagValueType.Bool:
                    return source.Add(tagKey, (bool) tagValue.Source);
                case TagValueType.Float:
                    return source.Add(tagKey, (float) tagValue.Source);
                case TagValueType.Double:
                    return source.Add(tagKey, (double) tagValue.Source);
                case TagValueType.String:
                    return source.Add(tagKey, (string) tagValue.Source);
                case TagValueType.ByteArray:
                    return source.Add(tagKey, (byte[]) tagValue.Source);
                case TagValueType.ShortArray:
                    return source.Add(tagKey, (short[]) tagValue.Source);
                case TagValueType.IntArray:
                    return source.Add(tagKey, (int[]) tagValue.Source);
                case TagValueType.LongArray:
                    return source.Add(tagKey, (long[]) tagValue.Source);
                case TagValueType.BoolArray:
                    return source.Add(tagKey, (bool[]) tagValue.Source);
                case TagValueType.FloatArray:
                    return source.Add(tagKey, (float[]) tagValue.Source);
                case TagValueType.DoubleArray:
                    return source.Add(tagKey, (double[]) tagValue.Source);
                case TagValueType.StringArray:
                    return source.Add(tagKey, (string[]) tagValue.Source);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tagValue.Type));
            }
        }
    }
}