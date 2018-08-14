using System;
using System.Threading;
using NUnit.Framework;
using Vostok.Logging.ConsoleLog;

namespace Vostok.Airlock.Client.Tests
{
    public class AirlockGateClientTests
    {
        [Test]
        public void Test()
        {
            var config = new AirlockConfig
            {
                GateUri = new Uri(""),
                GateApiKey = ""
            };

            var client = new AirlockGateClient(new ConsoleLog(), config);

            client.Put("", x => { x.Add("key", true); });

            Thread.Sleep(1000000);
        }
    }
}