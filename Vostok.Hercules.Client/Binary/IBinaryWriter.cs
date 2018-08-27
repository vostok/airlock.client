using System.Text;

namespace Vostok.Hercules.Client.Binary
{
    internal interface IBinaryWriter
    {
        int Position { get; set; }

        IBinaryWriter Write(int value);
        IBinaryWriter Write(long value);
        IBinaryWriter Write(short value);
        IBinaryWriter Write(double value);
        IBinaryWriter Write(float value);
        IBinaryWriter Write(byte value);
        IBinaryWriter Write(bool value);
        IBinaryWriter Write(string value, Encoding encoding);
        IBinaryWriter Write(byte[] value, int offset, int length);
    }
}