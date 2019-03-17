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

            if (settings.MaximumRecordSize > settings.MaximumBatchSize)
                throw new ArgumentException($"Maximum record size {settings.MaximumRecordSize} is greater than maximum batch size {settings.MaximumBatchSize}.");
        }
    }
}
