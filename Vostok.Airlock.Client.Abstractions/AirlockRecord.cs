using System;
using System.Collections.Generic;

namespace Vostok.Airlock.Client.Abstractions
{
    public class AirlockRecord
    {
        public DateTimeOffset Timestamp { get; set; }

        public Dictionary<string, TagValue> Tags { get; set; }
    }
}