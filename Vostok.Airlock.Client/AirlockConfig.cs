using Vostok.Commons;

namespace Vostok.Airlock.Client
{
    public class AirlockConfig
    {
        public DataSize MaximumRecordSize { get; set; } = DataSize.FromMegabytes(1);

        public DataSize MaximumMemoryConsumption { get; set; } = DataSize.FromMegabytes(128);

        public DataSize InitialPooledBufferSize { get; set; } = DataSize.FromKilobytes(16);

        public int InitialPooledBuffersCount { get; set; } = 32;
    }
}