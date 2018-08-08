using System.Text;

namespace Vostok.Airlock.Client.Binary
{
    internal interface IBinaryWriter
    {
        long Position { get; set; }

        IBinaryWriter Write(int value);
        IBinaryWriter Write(long value);
        IBinaryWriter Write(short value);

        IBinaryWriter Write(double value);
        IBinaryWriter Write(float value);

        IBinaryWriter Write(byte value);
        IBinaryWriter Write(bool value);

        IBinaryWriter Write(string value, Encoding encoding);
        IBinaryWriter WriteWithoutLengthPrefix(string value, Encoding encoding);

        IBinaryWriter Write(byte[] value, int offset, int length);
        IBinaryWriter WriteWithoutLengthPrefix(byte[] value, int offset, int length);
    }
}