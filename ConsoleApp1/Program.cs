using System;
using System.Diagnostics;
using System.Threading;
using Vostok.Hercules.Client;
using Vostok.Logging.Console;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            { 
                var config = new HerculesConfig
                {
                    GateUri = new Uri("http://vm-hercules04:6306"),
                    GateApiKey = "dotnet_api_key",
                    RequestSendPeriod = TimeSpan.FromSeconds(1)
                };

                var client = new HerculesGateClient(config);

                var sw = Stopwatch.StartNew();

                for (int i = 0; i < 1000000; i++)
                {
                    client.Put(
                        "dotnet_test_0",
                        builder => builder
                            .Add("abcdzz", 123L)
                            .Add("abcde", 123L)
                            .Add("abcdef", 123L)
                            .Add("abcdefg", 123L)
                            .Add("abacafa", "avacada"));
                }

                Console.WriteLine(sw.Elapsed);
                
                for (int i = 0; i < 50; i++)
                {
                    Console.WriteLine(client.SentRecordsCount + " " + client.LostRecordsCount + " " + client.StoredRecordsCount);
                    Console.WriteLine(client.SentRecordsCount + client.LostRecordsCount + client.StoredRecordsCount);
                    Thread.Sleep(1000);
                }
            }
            finally
            {
                ConsoleLog.Flush();
            }
        }
    }
}