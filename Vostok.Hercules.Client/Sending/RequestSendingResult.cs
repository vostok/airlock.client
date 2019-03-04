using Vostok.Clusterclient.Core.Model;

namespace Vostok.Hercules.Client.Sending
{
    internal struct RequestSendingResult
    {
        public ClusterResultStatus Status;
        public ResponseCode Code;


        public bool IsSuccessful => Status == ClusterResultStatus.Success && Code == ResponseCode.Ok;

        public bool IsIntermittentFailure =>
            Status == ClusterResultStatus.TimeExpired ||
            Status == ClusterResultStatus.ReplicasExhausted ||
            Status == ClusterResultStatus.Throttled;


        public bool IsDefinitiveFailure => !IsSuccessful && !IsIntermittentFailure;
    }
}