using System;
using System.Threading;
using NUnit.Framework;

namespace Vostok.Hercules.Client.Tests
{
    public class HerculesGateClientTests
    {
        [Test]
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