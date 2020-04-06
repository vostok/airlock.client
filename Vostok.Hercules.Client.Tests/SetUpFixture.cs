using NUnit.Framework;
using Vostok.Commons.Threading;
using Vostok.Logging.Abstractions;
using Vostok.Logging.Console;

namespace Vostok.Hercules.Client.Tests
{
    [SetUpFixture]
    internal class SetUpFixture
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ThreadPoolUtility.Setup();
            LogProvider.Configure(new SynchronousConsoleLog());
        }
    }
}