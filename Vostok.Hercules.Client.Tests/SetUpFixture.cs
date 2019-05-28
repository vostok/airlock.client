using NUnit.Framework;
using Vostok.Commons.Threading;

namespace Vostok.Hercules.Client.Tests
{
    [SetUpFixture]
    internal class SetUpFixture
    {
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            ThreadPoolUtility.Setup();
        }
    }
}