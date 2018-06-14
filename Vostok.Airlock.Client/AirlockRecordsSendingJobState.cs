using System;

namespace Vostok.Airlock.Client
{
    internal class AirlockRecordsSendingJobState
    {
        public static AirlockRecordsSendingJobState Unknown => new AirlockRecordsSendingJobState {Result = true, Attempt = 0, Elapsed = TimeSpan.Zero};

        public bool Result { get; set; }
        public int Attempt { set; get; }
        public TimeSpan Elapsed { get; set; }
    }
}