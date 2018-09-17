namespace Vostok.Hercules.Client.TimeBasedUuid
{
    internal class TimeGuid
    {
        public const int Size = 16;

        public static TimeGuid Empty { get; } = new TimeGuid(new byte[Size]);

        private readonly byte[] bytes;

        public TimeGuid(byte[] bytes) => this.bytes = bytes;

        public static implicit operator byte[](TimeGuid timeGuid) => timeGuid.bytes;
    }
}