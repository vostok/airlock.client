using System;
using System.Collections;
using System.Net;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client.Tests
{
    [TestFixture]
    public class HerculesRecordPayloadBuilderWithCounterTests
    {
        [TestCaseSource(nameof(TestCases))]
        public void IncrementsCounter_WhenAdd(Action<IHerculesRecordPayloadBuilder> add, int addedBytesLength)
        {
            var writer = CreateWriter();
            var builder = CreateBuilder(writer);

            add.Invoke(builder);

            builder.Dispose();

            Assert.That(writer.Position, Is.EqualTo(sizeof(short) + addedBytesLength));
            Assert.That(BitConverter.ToInt16(writer.Buffer, 0), Is.EqualTo(IPAddress.HostToNetworkOrder((short) 1)));
        }

        private static IEnumerable TestCases()
        {
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", x => x.Add("nested", 0))), 19).SetName("Func");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", (byte) 0)), 6).SetName("Byte");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", (short) 0)), 7).SetName("Int16");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", 0)), 9).SetName("Int32");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", 0L)), 13).SetName("Int64");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", true)), 6).SetName("Bool");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", 0F)), 9).SetName("Float");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", 0D)), 13).SetName("Double");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", "value")), 11).SetName("String");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new Func<IHerculesRecordPayloadBuilder, IHerculesRecordPayloadBuilder>[] {x => x.Add("nested", 0)})), 20).SetName("FuncArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {(byte) 0})), 7).SetName("ByteArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {(short) 0})), 8).SetName("Int16Array");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {0})), 10).SetName("Int32Array");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {0L})), 14).SetName("Int64Array");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {true})), 7).SetName("BoolArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {0F})), 10).SetName("FloatArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {0D})), 14).SetName("DoubleArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {"value"})), 12).SetName("StringArray");
        }

        private static BinaryBufferWriter CreateWriter() => new BinaryBufferWriter(0);

        private static HerculesRecordPayloadBuilderWithCounter CreateBuilder(IBinaryWriter writer) => new HerculesRecordPayloadBuilderWithCounter(writer);
    }
}