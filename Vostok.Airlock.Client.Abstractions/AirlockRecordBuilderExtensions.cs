using System;
using System.Collections.Generic;
using System.Linq;

namespace Vostok.Airlock.Client.Abstractions
{
    public static class AirlockRecordBuilderExtensions
    {
        public static IAirlockRecordBuilder Add(this IAirlockRecordBuilder source, Dictionary<string, TagValue> tags) =>
            tags.Aggregate(source, (builder, tag) => builder.Add(tag.Key, tag.Value));

        public static IAirlockRecordBuilder Add(this IAirlockRecordBuilder source, string tagKey, TagValue tagValue)
        {
            switch (tagValue.Type)
            {
                case TagValueType.Byte:
                    return source.Add(tagKey, (byte)tagValue);
                case TagValueType.Short:
                    return source.Add(tagKey, (short)tagValue);
                case TagValueType.Int:
                    return source.Add(tagKey, (int)tagValue);
                case TagValueType.Long:
                    return source.Add(tagKey, (long)tagValue);
                case TagValueType.Bool:
                    return source.Add(tagKey, (bool)tagValue);
                case TagValueType.Float:
                    return source.Add(tagKey, (float)tagValue);
                case TagValueType.Double:
                    return source.Add(tagKey, (double)tagValue);
                case TagValueType.String:
                    return source.Add(tagKey, (string)tagValue);
                case TagValueType.ByteArray:
                    return source.Add(tagKey, (byte[])tagValue);
                case TagValueType.ShortArray:
                    return source.Add(tagKey, (short[])tagValue);
                case TagValueType.IntArray:
                    return source.Add(tagKey, (int[])tagValue);
                case TagValueType.LongArray:
                    return source.Add(tagKey, (long[])tagValue);
                case TagValueType.BoolArray:
                    return source.Add(tagKey, (bool[])tagValue);
                case TagValueType.FloatArray:
                    return source.Add(tagKey, (float[])tagValue);
                case TagValueType.DoubleArray:
                    return source.Add(tagKey, (double[])tagValue);
                case TagValueType.StringArray:
                    return source.Add(tagKey, (string[])tagValue);
                default:
                    throw new ArgumentOutOfRangeException(nameof(tagValue.Type));
            }
        }
    }
}