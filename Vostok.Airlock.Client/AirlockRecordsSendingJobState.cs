using System;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordsSendingJobState
    {
        public bool IsSuccess { get; set; }
        public int Attempt { set; get; }
        public TimeSpan Elapsed { get; set; }
    }
}