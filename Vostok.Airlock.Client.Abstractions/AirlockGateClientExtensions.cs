namespace Vostok.Airlock.Client.Abstractions
{
    public static class AirlockGateClientExtensions
    {
        public static void Put(this IAirlockGateClient client, string stream, AirlockRecord record) =>
            client.Put(stream, builder => builder.SetTimestamp(record.Timestamp).Add(record.Tags));
    }
}