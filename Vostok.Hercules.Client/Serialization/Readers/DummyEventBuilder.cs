using System;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client.Serialization.Readers
{
    internal class DummyEventBuilder : DummyHerculesTagsBuilder, IHerculesEventBuilder<HerculesEvent>
    {
        public static readonly DummyEventBuilder Instance = new DummyEventBuilder();

        private static readonly HerculesEvent DummyEvent = new HerculesEvent(DateTimeOffset.MinValue, HerculesTags.Empty);

        public IHerculesEventBuilder<HerculesEvent> SetTimestamp(DateTimeOffset timestamp) => this;

        public HerculesEvent BuildEvent() => DummyEvent;
    }
}