using System;
using Vostok.Commons.Primitives;
using Vostok.Commons.Helpers.Conversions;

namespace Vostok.Airlock.Client
{
    public class AirlockConfig
    {
        public DataSize MaximumRecordSize { get; set; } = 1.Megabytes();
        public DataSize MaximumMemoryConsumption { get; set; } = 128.Megabytes();
        public DataSize InitialPooledBufferSize { get; set; } = 16.Kilobytes();
        public int InitialPooledBuffersCount { get; set; } = 32;
        public DataSize MaximumRequestContentSize { get; set; } = 4.Megabytes();
        public TimeSpan RequestSendPeriod { get; set; } = 2.Seconds();
        public TimeSpan RequestSendPeriodCap { get; set; } = 1.Minutes();
        public TimeSpan RequestTimeout { get; set; } = 30.Seconds();
        public string GateName { get; set; } = "AirlockGateway";
        public Uri GateUri { get; set; }
        public string GateApiKey { get; set; }
    }
}