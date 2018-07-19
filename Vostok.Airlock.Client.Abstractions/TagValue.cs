using System;

namespace Vostok.Airlock.Client.Abstractions
{
    public class TagValue
    {
        public TagValue(object value)
        {
            UnderlyingValue = value;
            Type = GetValueType(value);
        }

        public object UnderlyingValue { get; }

        public TagValueType Type { get; }

        public static explicit operator byte(TagValue source) => (byte)source.UnderlyingValue;

        public static explicit operator short(TagValue source) => (short)source.UnderlyingValue;

        public static explicit operator int(TagValue source) => (int)source.UnderlyingValue;

        public static explicit operator long(TagValue source) => (long)source.UnderlyingValue;

        public static explicit operator bool(TagValue source) => (bool)source.UnderlyingValue;

        public static explicit operator float(TagValue source) => (float)source.UnderlyingValue;

        public static explicit operator double(TagValue source) => (double)source.UnderlyingValue;

        public static explicit operator string(TagValue source) => (string)source.UnderlyingValue;

        public static explicit operator byte[](TagValue source) => (byte[])source.UnderlyingValue;

        public static explicit operator short[](TagValue source) => (short[])source.UnderlyingValue;

        public static explicit operator int[](TagValue source) => (int[])source.UnderlyingValue;

        public static explicit operator long[](TagValue source) => (long[])source.UnderlyingValue;

        public static explicit operator bool[](TagValue source) => (bool[])source.UnderlyingValue;

        public static explicit operator float[](TagValue source) => (float[])source.UnderlyingValue;

        public static explicit operator double[](TagValue source) => (double[])source.UnderlyingValue;

        public static explicit operator string[](TagValue source) => (string[])source.UnderlyingValue;

        private static TagValueType GetValueType(object value)
        {
            switch (value)
            {
                case byte _:
                    return TagValueType.Byte;
                case short _:
                    return TagValueType.Short;
                case int _:
                    return TagValueType.Int;
                case long _:
                    return TagValueType.Long;
                case bool _:
                    return TagValueType.Bool;
                case float _:
                    return TagValueType.Float;
                case double _:
                    return TagValueType.Double;
                case string _:
                    return TagValueType.String;
                case byte[] _:
                    return TagValueType.ByteArray;
                case short[] _:
                    return TagValueType.ShortArray;
                case int[] _:
                    return TagValueType.IntArray;
                case long[] _:
                    return TagValueType.LongArray;
                case bool[] _:
                    return TagValueType.BoolArray;
                case float[] _:
                    return TagValueType.FloatArray;
                case double[] _:
                    return TagValueType.DoubleArray;
                case string[] _:
                    return TagValueType.StringArray;
                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }
        }
    }
}