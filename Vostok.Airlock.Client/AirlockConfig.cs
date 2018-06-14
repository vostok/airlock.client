using System;
using Vostok.Commons;
using Vostok.Commons.Conversions;

namespace Vostok.Airlock.Client
{
    public class AirlockConfig
    {
        public DataSize MaximumRecordSize { get; set; } = DataSize.FromMegabytes(1);

        public DataSize MaximumMemoryConsumption { get; set; } = DataSize.FromMegabytes(128);


        public DataSize InitialPooledBufferSize { get; set; } = DataSize.FromKilobytes(16);

        public int InitialPooledBuffersCount { get; set; } = 32;


        public DataSize MaximumRequestContentSize { get; set; } = DataSize.FromMegabytes(4);

        public TimeSpan RequestSendPeriod { get; set; } = 2.Seconds();

        public TimeSpan RequestSendPeriodCap { get; set; } = 1.Minutes();
    }
}