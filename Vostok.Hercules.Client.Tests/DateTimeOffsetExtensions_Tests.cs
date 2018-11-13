using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using NUnit.Framework;

namespace Vostok.Hercules.Client.Tests
{
    internal class DateTimeOffsetExtensions_Tests
    {
        [Test]
        public void ToUnixTimeNanoseconds_should_return_zero_for_unix_epoch_start()
        {
            var dateTime = new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var nanoseconds = dateTime.ToUnixTimeNanoseconds();

            nanoseconds.Should().Be(0);
        }
        
        [Test]
        public void ToUnixTimeNanoseconds_should_return_correct_value()
        {
            var dateTime = new DateTimeOffset(1971, 2, 3, 4, 5, 6, TimeSpan.Zero);

            var timeFromEpochStart = 365.Days() + 31.Days() + 2.Days() + 4.Hours() + 5.Minutes() + 6.Seconds();
            var expected = (long) timeFromEpochStart.TotalMilliseconds * 1000 * 1000;
            
            var nanoseconds = dateTime.ToUnixTimeNanoseconds();

            nanoseconds.Should().Be(expected);
        }
    }
}