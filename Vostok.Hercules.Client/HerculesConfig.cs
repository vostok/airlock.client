using System;
using Vostok.Commons.Helpers.Conversions;
using Vostok.Commons.Primitives;

namespace Vostok.Hercules.Client
{
    public class HerculesConfig
    {
        public byte RecordVersion => 1;
        public DataSize MaximumRecordSize { get; set; } = 1.Megabytes();
        public DataSize MaximumMemoryConsumption { get; set; } = 128.Megabytes();
        public DataSize InitialPooledBufferSize { get; set; } = 16.Kilobytes();
        public int InitialPooledBuffersCount { get; set; } = 32;
        public DataSize MaximumRequestContentSize { get; set; } = 4.Megabytes();
        public TimeSpan RequestSendPeriod { get; set; } = 2.Seconds();
        public TimeSpan RequestSendPeriodCap { get; set; } = 1.Minutes();
        public TimeSpan RequestTimeout { get; set; } = 30.Seconds();
        public string GateName { get; set; } = "HerculesGateway";
        public Uri GateUri { get; set; }
        public string GateApiKey { get; set; }
    }
}