using System;
using Vostok.Commons.Conversions;
using Vostok.Commons.ValueObjects;

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
        public TimeSpan RequestTimeout { get; set; } = 30.Seconds();
        public string GateName { get; set; } = "AirlockGateway";
        public Uri GateUri { get; set; }
        public string GateApiKey { get; set; }
    }
}