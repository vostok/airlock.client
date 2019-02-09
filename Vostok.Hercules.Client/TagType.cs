namespace Vostok.Hercules.Client
{
    internal enum TagType : byte
    {
        Container = 0x01,
        Byte = 0x02,
        Short = 0x03,
        Integer = 0x04,
        Long = 0x05,
        Flag = 0x06,
        Float = 0x07,
        Double = 0x08,
        String = 0x09,
        UUID = 0x0A,
        Null = 0x0B,
        Vector = 0x80
    }
}