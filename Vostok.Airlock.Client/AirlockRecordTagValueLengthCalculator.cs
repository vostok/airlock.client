using System;
using System.Net;

namespace Vostok.Airlock.Client
{
    internal static class AirlockRecordTagValueLengthCalculator
    {
        public static int Calculate(byte[] buffer, int position, TagValueTypeDefinition tagValueTypeDefinition)
        {
            switch (tagValueTypeDefinition)
            {
                case TagValueTypeDefinition.Byte:
                    return sizeof(byte);
                case TagValueTypeDefinition.Short:
                    return sizeof(short);
                case TagValueTypeDefinition.Integer:
                    return sizeof(int);
                case TagValueTypeDefinition.Long:
                    return sizeof(long);
                case TagValueTypeDefinition.Flag:
                    return sizeof(bool);
                case TagValueTypeDefinition.Float:
                    return sizeof(float);
                case TagValueTypeDefinition.Double:
                    return sizeof(double);
                case TagValueTypeDefinition.String:
                    return sizeof(byte) + buffer[position];
                case TagValueTypeDefinition.Text:
                    return sizeof(int) + ReadInt32(buffer, position);
                case TagValueTypeDefinition.ByteArray:
                    return sizeof(int) + ReadInt32(buffer, position);
                case TagValueTypeDefinition.ShortArray:
                    return sizeof(int) + ReadInt32(buffer, position) * sizeof(short);
                case TagValueTypeDefinition.IntegerArray:
                    return sizeof(int) + ReadInt32(buffer, position) * sizeof(int);
                case TagValueTypeDefinition.LongArray:
                    return sizeof(int) + ReadInt32(buffer, position) * sizeof(long);
                case TagValueTypeDefinition.FlagArray:
                    return sizeof(int) + ReadInt32(buffer, position) * sizeof(bool);
                case TagValueTypeDefinition.FloatArray:
                    return sizeof(int) + ReadInt32(buffer, position) * sizeof(float);
                case TagValueTypeDefinition.DoubleArray:
                    return sizeof(int) + ReadInt32(buffer, position) * sizeof(double);
                case TagValueTypeDefinition.StringArray:
                    var stringArrayLength = sizeof(int);
                    for (var i = 0; i < ReadInt32(buffer, position); i++)
                        stringArrayLength += Calculate(buffer, position + stringArrayLength, TagValueTypeDefinition.String);
                    return stringArrayLength;
                case TagValueTypeDefinition.TextArray:
                    var textArrayLength = sizeof(int);
                    for (var i = 0; i < ReadInt32(buffer, position); i++)
                        textArrayLength += Calculate(buffer, position + textArrayLength, TagValueTypeDefinition.Text);
                    return textArrayLength;
                case TagValueTypeDefinition.ByteVector:
                    return sizeof(byte) + buffer[position];
                case TagValueTypeDefinition.ShortVector:
                    return sizeof(byte) + buffer[position] * sizeof(short);
                case TagValueTypeDefinition.IntegerVector:
                    return sizeof(byte) + buffer[position] * sizeof(int);
                case TagValueTypeDefinition.LongVector:
                    return sizeof(byte) + buffer[position] * sizeof(long);
                case TagValueTypeDefinition.FlagVector:
                    return sizeof(byte) + buffer[position] * sizeof(bool);
                case TagValueTypeDefinition.FloatVector:
                    return sizeof(byte) + buffer[position] * sizeof(float);
                case TagValueTypeDefinition.DoubleVector:
                    return sizeof(byte) + buffer[position] * sizeof(double);
                case TagValueTypeDefinition.StringVector:
                    var stringVectorLength = sizeof(byte);
                    for (var i = 0; i < buffer[position]; i++)
                        stringVectorLength += Calculate(buffer, position + stringVectorLength, TagValueTypeDefinition.String);
                    return stringVectorLength;
                case TagValueTypeDefinition.TextVector:
                    var textVectorLength = sizeof(byte);
                    for (var i = 0; i < buffer[position]; i++)
                        textVectorLength += Calculate(buffer, position + textVectorLength, TagValueTypeDefinition.Text);
                    return textVectorLength;
                default:
                    throw new ArgumentOutOfRangeException(nameof(tagValueTypeDefinition));
            }
        }

        private static int ReadInt32(byte[] buffer, int position)
        {
            return IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, position));
        }
    }
}