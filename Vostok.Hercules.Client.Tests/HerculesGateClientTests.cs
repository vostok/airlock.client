using System;
using System.Threading;
using FluentAssertions;
using NUnit.Framework;
using Vostok.Commons.Testing;
using Vostok.Commons.Time;

namespace Vostok.Hercules.Client.Tests
{
    public class HerculesGateClientTests
    {
        [Test]
        public void LostRecordsCount_should_not_grows_infinitely_when_gate_is_offline()
        {
            var config = new HerculesConfig
            {
                GateUri = new Uri("http://example.com/dev/null"),
                GateApiKey = ""
            };

            var client = new HerculesGateClient(config);

            client.Put("", x => x.Add("key", true));

            var action = new Action(() => client.LostRecordsCount.Should().Be(1));
            action.ShouldPassIn(5.Seconds());
            action.ShouldNotFailIn(3.Seconds());
        }

        [Test, Explicit]
        public void Test()
        {
            var config = new HerculesConfig
            {
                GateUri = new Uri(""),
                GateApiKey = ""
            };

            var client = new HerculesGateClient(config);

            client.Put("", x => { x.Add("key", true); });

            Thread.Sleep(1000000);
        }
    }
}