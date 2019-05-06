using System;
using System.Collections.Generic;
using System.Linq;
using Vostok.Hercules.Client.Abstractions.Events;

namespace Vostok.Hercules.Client.Tests.Functional.Helpers
{
    internal static class ActionHerculesEventBuilderExtensions
    {
        public static HerculesEvent ToEvent(this Action<IHerculesEventBuilder> eventBuilder)
        {
            var expectedEventBuilder = new HerculesEventBuilder();
            eventBuilder(expectedEventBuilder);
            return expectedEventBuilder.BuildEvent();
        }

        public static HerculesEvent[] ToEvents(this IEnumerable<Action<IHerculesEventBuilder>> eventBuilder) =>
            eventBuilder.Select(ToEvent).ToArray();

        public static void PushEvents(
            this IEnumerable<Action<IHerculesEventBuilder>> builders,
            Action<Action<IHerculesEventBuilder>> pushEvent,
            int degreeOfParallelism = 1)
        {
            builders
                .AsParallel()
                .WithDegreeOfParallelism(degreeOfParallelism)
                .ForAll(pushEvent);
        }

        public static Action<Action<IHerculesEventBuilder>> ToStream(
            this Action<string, Action<IHerculesEventBuilder>> pushEvent,
            string stream) => e => pushEvent(stream, e);
    }
}