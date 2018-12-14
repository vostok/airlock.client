// using System;
// using System.Linq;
// using FluentAssertions;
// using NSubstitute;
// using NUnit.Framework;
// using Vostok.Hercules.Client.Binary;
//
// namespace Vostok.Hercules.Client.Tests
// {
//     [TestFixture]
//     public class HerculesRecordBuilderTests
//     {
//         [Test]
//         public void Dispose_WhenSetTimestampCalled_WritesTimeGuid()
//         {
//             var timeGuid = new TimeGuid(new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15});
//
//             var timeGuidGenerator = CreateTimeGuidGenerator();
//             timeGuidGenerator.NewGuid(Arg.Any<long>()).Returns(timeGuid);
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer, timeGuidGenerator);
//
//             builder.SetTimestamp(DateTimeOffset.UtcNow);
//
//             builder.Dispose();
//
//             writer.Buffer.Take(TimeGuid.Size).Should().BeEquivalentTo((byte[]) timeGuid);
//         }
//
//         [Test]
//         public void Dispose_WhenSetTimestampNotCalled_WritesTimeGuid()
//         {
//             var timeGuid = new TimeGuid(new byte[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15});
//
//             var timeGuidGenerator = CreateTimeGuidGenerator();
//             timeGuidGenerator.NewGuid().Returns(timeGuid);
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer, timeGuidGenerator);
//
//             builder.Dispose();
//
//             writer.Buffer.Take(TimeGuid.Size).Should().BeEquivalentTo((byte[]) timeGuid);
//         }
//
//         private static ITimeGuidGenerator CreateTimeGuidGenerator() => Substitute.For<ITimeGuidGenerator>();
//
//         private static HerculesBinaryBufferWriter CreateWriter() => new HerculesBinaryBufferWriter(0);
//
//         private static HerculesEventBuilder CreateBuilder(IHerculesBinaryWriter writer, ITimeGuidGenerator timeGuidGenerator) => new HerculesEventBuilder(writer, timeGuidGenerator);
//     }
// }