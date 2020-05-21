namespace Vostok.Hercules.Client
{
    internal static class Constants
    {
        public const byte EventProtocolVersion = 1;

        internal static class ServiceNames
        {
            public const string Gate = "Hercules.Gate";
            public const string StreamApi = "Hercules.StreamApi";
            public const string TimelineApi = "Hercules.TimelineApi";
            public const string ManagementApi = "Hercules.ManagementApi";
        }

        internal static class QueryParameters
        {
            public const string Stream = "stream";
            public const string Timeline = "timeline";
            public const string ClientShard = "shardIndex";
            public const string ClientShardCount = "shardCount";
            public const string Limit = "take";
            public const string FetchTimeoutMs = "timeoutMs";
        }

        internal static class HeaderNames
        {
            public const string ApiKey = "apiKey";
        }

        internal static class ContentTypes
        {
            public const string OctetStream = "application/octet-stream";
            public const string Json = "application/json";
        }

        internal static class Compression
        {
            public const string Lz4Encoding = "lz4";

            public const string OriginalContentLengthHeaderName = "Original-Content-Length";
        }
    }
}