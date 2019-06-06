using System;
using Vostok.Clusterclient.Core.Model;
using Vostok.Clusterclient.Core.Transforms;

namespace Vostok.Hercules.Client.Client
{
    internal class ApiKeyRequestTransform : IRequestTransform
    {
        private readonly Func<string> apiKeyProvider;

        public ApiKeyRequestTransform(Func<string> apiKeyProvider)
        {
            this.apiKeyProvider = apiKeyProvider ?? throw new ArgumentNullException(nameof(apiKeyProvider));
        }

        public Request Transform(Request request)
        {
            var key = apiKeyProvider();
            return key == null 
                ? request 
                : request.WithHeader(Constants.HeaderNames.ApiKey, apiKeyProvider());
        }
    }
}