using System;

namespace Vostok.Hercules.Client.Sink.Validation
{
    internal static class SettingsValidator
    {
        public static void Validate(HerculesSinkSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            if (settings.MaximumMemoryConsumption <= 0)
                throw new ArgumentException($"Maximum memory consumption has incorrect value {settings.MaximumMemoryConsumption}");

            if (settings.MaximumPerStreamMemoryConsumption <= 0)
                throw new ArgumentException($"Maximum per-stream memory consumption has incorrect value {settings.MaximumPerStreamMemoryConsumption}");

            if (settings.MaximumBatchSize <= 0)
                throw new ArgumentException($"Maximum batch size has incorrect value {settings.MaximumBatchSize}");

            if (settings.MaximumRecordSize <= 0)
                throw new ArgumentException($"Maximum record size has incorrect value {settings.MaximumRecordSize}");

            if (settings.MaxParallelStreams <= 0)
                throw new ArgumentException($"Max parallel streams has incorrect value {settings.MaxParallelStreams}");

            if (settings.SendPeriod <= TimeSpan.Zero)
                throw new ArgumentException($"Send period has incorrect value {settings.SendPeriod}");

            if (settings.SendPeriodCap <= TimeSpan.Zero)
                throw new ArgumentException($"Send period cap has incorrect value {settings.SendPeriodCap}");

            if (settings.MaximumPerStreamMemoryConsumption > settings.MaximumMemoryConsumption)
                throw new ArgumentException($"Maximum per-stream memory consumption {settings.MaximumPerStreamMemoryConsumption} is greater than maximum memory consumption {settings.MaximumMemoryConsumption}");

            if (settings.MaximumRecordSize > settings.MaximumBatchSize)
                throw new ArgumentException($"Maximum record size {settings.MaximumRecordSize} is greater than maximum batch size {settings.MaximumBatchSize}.");

            if (settings.SendPeriod > settings.SendPeriodCap)
                throw new ArgumentException($"Send period {settings.SendPeriod} is greater than send period cap {settings.SendPeriodCap}.");

            if (settings.Cluster == null)
                throw new ArgumentNullException(nameof(settings.Cluster));

            if (settings.ApiKeyProvider == null)
                throw new ArgumentNullException(nameof(settings.ApiKeyProvider));
        }
    }
}