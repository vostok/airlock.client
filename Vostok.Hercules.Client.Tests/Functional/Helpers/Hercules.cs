using System;
using FluentAssertions.Extensions;
using NUnit.Framework;
using Vostok.Commons.Collections;
using Vostok.Hercules.Local;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Hercules.Client.Tests.Functional.Helpers
{
    public class Hercules : IDisposable
    {
        private readonly HerculesCluster cluster;
        private readonly ILog log;

        public Hercules()
        {
            log = new SynchronousConsoleLog();

            cluster = HerculesCluster.DeployNew(TestContext.CurrentContext.TestDirectory, log.WithMinimumLevel(LogLevel.Warn));

            string GetApiKey() => cluster.ApiKey;

            var managementSettings = new HerculesManagementClientSettings(
                cluster.HerculesManagementApiTopology,
                GetApiKey);

            var streamSettings = new HerculesStreamClientSettings(
                cluster.HerculesStreamApiTopology,
                GetApiKey);

            var gateSettings = new HerculesGateClientSettings(
                cluster.HerculesGateTopology,
                GetApiKey);

            var sinkSettings = new HerculesSinkSettings(
                cluster.HerculesGateTopology,
                GetApiKey)
            {
                SendPeriod = 1.Seconds()
            };

            Management = new HerculesManagementClient(
                managementSettings,
                log);

            Sink = new HerculesSink(sinkSettings, log);

            Stream = new HerculesStreamClient(streamSettings, log);

            Gate = new HerculesGateClient(gateSettings, log);
        }

        public HerculesSink Sink { get; }
        public HerculesManagementClient Management { get; }
        public HerculesGateClient Gate { get; }
        public HerculesStreamClient Stream { get; }

        public void Dispose()
        {
            log.Info("Rented in BufferPools: {Rented}.", BufferPool.Rented);

            Sink?.Dispose();
            cluster?.Dispose();
        }
    }
}