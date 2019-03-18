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
        }

        internal static class HeaderNames
        {
            public const string ApiKey = "apiKey";
        }

        internal static class ContentTypes
        {
            public const string OctetStream = "application/octet-stream";
        }
    }
}