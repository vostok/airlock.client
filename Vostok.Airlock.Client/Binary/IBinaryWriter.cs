using System;
using System.Text;

namespace Vostok.Airlock.Client.Binary
{
    internal interface IBinaryWriter
    {
        long Position { get; set; }

        IBinaryWriter Write(int value);
        IBinaryWriter Write(long value);
        IBinaryWriter Write(short value);

        IBinaryWriter Write(uint value);
        IBinaryWriter Write(ulong value);
        IBinaryWriter Write(ushort value);

        IBinaryWriter Write(byte value);
        IBinaryWriter Write(bool value);
        IBinaryWriter Write(float value);
        IBinaryWriter Write(double value);
        IBinaryWriter Write(Guid value);

        IBinaryWriter Write(string value, Encoding encoding);
        IBinaryWriter WriteWithoutLengthPrefix(string value, Encoding encoding);

        IBinaryWriter Write(byte[] value);
        IBinaryWriter Write(byte[] value, int offset, int length);
        IBinaryWriter WriteWithoutLengthPrefix(byte[] value);
        IBinaryWriter WriteWithoutLengthPrefix(byte[] value, int offset, int length);
    }
}