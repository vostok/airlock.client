using System;
using System.Text;

namespace Vostok.Hercules.Client.Binary
{
    internal interface IHerculesBinaryWriter
    {
        int Position { get; set; }
        bool IsOverflowed { get; set; }
        byte[] Array { get; }
        ArraySegment<byte> FilledSegment { get; }
        Encoding Encoding { get; }
        
        void Write(int value);
        void Write(long value);
        void Write(short value);
        void Write(double value);
        void Write(float value);
        void Write(byte value);
        void Write(bool value);
        void Write(ushort value);
        void Write(Guid value);
        void WriteWithoutLength(string value);
        void WriteWithLength(string value);
        void WriteWithLength(byte[] value, int offset, int length);
        void WriteWithoutLength(byte[] value, int offset, int length);
    }
}