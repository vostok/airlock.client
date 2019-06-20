using System;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client.Serialization.Readers
{
    internal class HerculesEventBuilderGeneric : HerculesEventBuilder, IHerculesEventBuilder<HerculesEvent>
    {
        public new IHerculesEventBuilder<HerculesEvent> SetTimestamp(DateTimeOffset timestamp)
        {
            base.SetTimestamp(timestamp);
            return this;
        }
    }
}