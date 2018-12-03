using System;
using System.Collections;
using System.Net;
using NUnit.Framework;
using Vostok.Hercules.Client.Abstractions;
using Vostok.Hercules.Client.Abstractions.Events;
using Vostok.Hercules.Client.Binary;

namespace Vostok.Hercules.Client.Tests
{
    [TestFixture]
    public class HerculesRecordPayloadBuilderWithCounterTests
    {
        [TestCaseSource(nameof(TestCases))]
        public void Dispose_WhenAddCalled_WritesCount(Action<IHerculesTagsBuilder> add)
        {
            var writer = CreateWriter();
            var builder = CreateBuilder(writer);

            add.Invoke(builder);

            builder.Dispose();
            Assert.That(BitConverter.ToInt16(writer.Buffer, 0), Is.EqualTo(IPAddress.HostToNetworkOrder((short) 1)));
        }

        private static IEnumerable TestCases()
        {
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddContainer("key", x => x.AddValue("nested", 0)))).SetName("Func");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddValue("key", (byte) 0))).SetName("Byte");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddValue("key", (short) 0))).SetName("Int16");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddValue("key", 0))).SetName("Int32");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddValue("key", 0L))).SetName("Int64");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddValue("key", true))).SetName("Bool");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddValue("key", 0F))).SetName("Float");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddValue("key", 0D))).SetName("Double");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddValue("key", "value"))).SetName("String");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddArrayOfContainers("key", new Action<IHerculesTagsBuilder>[] {x => x.AddValue("nested", 0)}))).SetName("FuncArray");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddArray("key", new[] {(byte) 0}))).SetName("ByteArray");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddArray("key", new[] {(short) 0}))).SetName("Int16Array");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddArray("key", new[] {0}))).SetName("Int32Array");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddArray("key", new[] {0L}))).SetName("Int64Array");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddArray("key", new[] {true}))).SetName("BoolArray");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddArray("key", new[] {0F}))).SetName("FloatArray");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddArray("key", new[] {0D}))).SetName("DoubleArray");
            yield return new TestCaseData((Action<IHerculesTagsBuilder>) (builder => builder.AddArray("key", new[] {"value"}))).SetName("StringArray");
        }

        private static BinaryBufferWriter CreateWriter() => new BinaryBufferWriter(0);

        private static HerculesRecordPayloadBuilderWithCounter CreateBuilder(IBinaryWriter writer) => new HerculesRecordPayloadBuilderWithCounter(writer);
    }
}