using Vostok.Clusterclient.Core.Model;
using Vostok.Hercules.Client.Abstractions.Results;

namespace Vostok.Hercules.Client.Client
{
    internal class ResponseAnalyzer
    {
        private const int MaximumErrorMessageLength = 250;

        private readonly ResponseAnalysisContext context;

        public ResponseAnalyzer(ResponseAnalysisContext context)
        {
            this.context = context;
        }

        public HerculesStatus Analyze(Response response, out string errorMessage)
        {
            var status = GetStatus(response);

            ExtractErrorMessage(response, status, out errorMessage);

            return status;
        }

        private HerculesStatus GetStatus(Response response)
        {
            switch (response.Code)
            {
                case ResponseCode.Ok:
                    return HerculesStatus.Success;

                case ResponseCode.BadRequest:
                    return HerculesStatus.IncorrectRequest;

                case ResponseCode.Unauthorized:
                    return HerculesStatus.Unauthorized;

                case ResponseCode.Forbidden:
                    return HerculesStatus.InsufficientPermissions;

                case ResponseCode.NotFound:
                    switch (context)
                    {
                        case ResponseAnalysisContext.Stream:
                            return HerculesStatus.StreamNotFound;

                        case ResponseAnalysisContext.Timeline:
                            return HerculesStatus.TimelineNotFound;
                    }
                    break;

                case ResponseCode.Conflict:
                    switch (context)
                    {
                        case ResponseAnalysisContext.Stream:
                            return HerculesStatus.StreamAlreadyExists;

                        case ResponseAnalysisContext.Timeline:
                            return HerculesStatus.TimelineAlreadyExists;
                    }
                    break;

                case ResponseCode.RequestEntityTooLarge:
                    return HerculesStatus.RequestTooLarge;

                case ResponseCode.Canceled:
                    return HerculesStatus.Canceled;

                case ResponseCode.TooManyRequests:
                    return HerculesStatus.Throttled;
                
                case ResponseCode.RequestTimeout:
                    return HerculesStatus.Timeout;

                case ResponseCode.ConnectFailure:
                case ResponseCode.SendFailure:
                case ResponseCode.ReceiveFailure:
                case ResponseCode.BadGateway:
                case ResponseCode.ProxyTimeout:
                case ResponseCode.StreamInputFailure:
                case ResponseCode.StreamReuseFailure:
                case ResponseCode.InsufficientStorage:
                    return HerculesStatus.NetworkError;

                case ResponseCode.InternalServerError:
                case ResponseCode.ServiceUnavailable:
                    return HerculesStatus.ServerError;
            }

            return HerculesStatus.UnknownError;
        }

        private void ExtractErrorMessage(Response response, HerculesStatus status, out string errorMessage)
        {
            errorMessage = null;

            if (status == HerculesStatus.Success)
                return;

            if (!response.HasContent)
                return;

            if (response.Content.Length > MaximumErrorMessageLength)
                return;

            errorMessage = response.Content.ToString();
        }
    }
}