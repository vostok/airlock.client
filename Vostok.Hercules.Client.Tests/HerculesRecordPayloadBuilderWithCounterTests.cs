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
        public void Dispose_WhenAddCalled_WritesCount(Action<IHerculesRecordPayloadBuilder> add)
        {
            var writer = CreateWriter();
            var builder = CreateBuilder(writer);

            add.Invoke(builder);

            builder.Dispose();
            Assert.That(BitConverter.ToInt16(writer.Buffer, 0), Is.EqualTo(IPAddress.HostToNetworkOrder((short) 1)));
        }

        private static IEnumerable TestCases()
        {
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", x => x.Add("nested", 0)))).SetName("Func");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", (byte) 0))).SetName("Byte");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", (short) 0))).SetName("Int16");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", 0))).SetName("Int32");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", 0L))).SetName("Int64");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", true))).SetName("Bool");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", 0F))).SetName("Float");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", 0D))).SetName("Double");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", "value"))).SetName("String");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new Func<IHerculesRecordPayloadBuilder, IHerculesRecordPayloadBuilder>[] {x => x.Add("nested", 0)}))).SetName("FuncArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {(byte) 0}))).SetName("ByteArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {(short) 0}))).SetName("Int16Array");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {0}))).SetName("Int32Array");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {0L}))).SetName("Int64Array");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {true}))).SetName("BoolArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {0F}))).SetName("FloatArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {0D}))).SetName("DoubleArray");
            yield return new TestCaseData((Action<IHerculesRecordPayloadBuilder>) (builder => builder.Add("key", new[] {"value"}))).SetName("StringArray");
        }

        private static BinaryBufferWriter CreateWriter() => new BinaryBufferWriter(0);

        private static HerculesRecordPayloadBuilderWithCounter CreateBuilder(IBinaryWriter writer) => new HerculesRecordPayloadBuilderWithCounter(writer);
    }
}