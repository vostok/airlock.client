using System;

namespace Vostok.Airlock.Client.Abstractions
{
    public interface IAirlockGateClient
    {
        void Put(string stream, Action<IAirlockRecordBuilder> build);
    }
}