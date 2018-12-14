// using System;
// using System.Linq;
// using NUnit.Framework;
// using Vostok.Hercules.Client.Abstractions;
// using Vostok.Hercules.Client.Abstractions.Events;
// using Vostok.Hercules.Client.Binary;
//
// namespace Vostok.Hercules.Client.Tests
// {
//     [TestFixture]
//     public class HerculesRecordPayloadBuilderTests
//     {
//         [Test]
//         public void Add_ValueIsFunc_Type()
//         {
//             Action<IHerculesTagsBuilder> value = x => x.AddValue("nested", 0);
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddContainer("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Container));
//         }
//
//         [Test]
//         public void Add_ValueIsFunc_Format()
//         {
//             Action<IHerculesTagsBuilder> value = x => x.AddValue("nested", 0);
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddContainer("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 2 + 1 + 6 + 1 + 4));
//         }
//
//         [Test]
//         public void Add_ValueIsByte_Type()
//         {
//             const byte value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Byte));
//         }
//
//         [Test]
//         public void Add_ValueIsByte_Format()
//         {
//             const byte value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1));
//         }
//
//         [Test]
//         public void Add_ValueIsInt16_Type()
//         {
//             const short value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Short));
//         }
//
//         [Test]
//         public void Add_ValueInt16_Format()
//         {
//             const short value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 2));
//         }
//
//         [Test]
//         public void Add_ValueIsInt32_Type()
//         {
//             const int value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Integer));
//         }
//
//         [Test]
//         public void Add_ValueIsInt32_Format()
//         {
//             const int value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4));
//         }
//
//         [Test]
//         public void Add_ValueIsInt64_Type()
//         {
//             const long value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Long));
//         }
//
//         [Test]
//         public void Add_ValueIsInt64_Format()
//         {
//             const long value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 8));
//         }
//
//         [Test]
//         public void Add_ValueIsBool_Type()
//         {
//             const bool value = true;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Flag));
//         }
//
//         [Test]
//         public void Add_ValueIsBool_Format()
//         {
//             const bool value = true;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1));
//         }
//
//         [Test]
//         public void Add_ValueIsFloat_Type()
//         {
//             const float value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Float));
//         }
//
//         [Test]
//         public void Add_ValueIsFloat_Format()
//         {
//             const float value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4));
//         }
//
//         [Test]
//         public void Add_ValueIsDouble_Type()
//         {
//             const double value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Double));
//         }
//
//         [Test]
//         public void Add_ValueIsDouble_Format()
//         {
//             const double value = 0;
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 8));
//         }
//
//         [Test]
//         public void Add_ValueIsString_WhenShort_Type()
//         {
//             const string value = " ";
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.String));
//         }
//
//         [Test]
//         public void Add_ValueIsString_WhenShort_Format()
//         {
//             const string value = " ";
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 1));
//         }
//
//         [Test]
//         public void Add_ValueIsString_WhenLong_Type()
//         {
//             var value = new string(' ', 256);
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Text));
//         }
//
//         [Test]
//         public void Add_ValueIsString_WhenLong_Format()
//         {
//             var value = new string(' ', 256);
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddValue("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + 256));
//         }
//
//         [Test]
//         public void Add_ValueIsFuncArray_WhenShort_Type()
//         {
//             var value = new Action<IHerculesTagsBuilder>[] {x => x.AddValue("nested", 0)};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVectorOfContainers("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.ContainerVector));
//         }
//
//         [Test]
//         public void Add_ValueIsFuncArray_WhenShort_Format()
//         {
//             var value = new Action<IHerculesTagsBuilder>[] {x => x.AddValue("nested", 0)};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVectorOfContainers("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 2 + 1 + 6 + 1 + 4));
//         }
//
//         [Test]
//         public void Add_ValueIsFuncArray_WhenLong_Type()
//         {
//             var value = Enumerable.Repeat((Action<IHerculesTagsBuilder>)(x => x.AddValue("nested", 0)), 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVectorOfContainers("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.ContainerArray));
//         }
//
//         [Test]
//         public void Add_ValueIsFuncArray_WhenLong_Format()
//         {
//             var value = Enumerable.Repeat((Action<IHerculesTagsBuilder>)(x => x.AddValue("nested", 0)), 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVectorOfContainers("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + (2 + 1 + 6 + 1 + 4) * 256));
//         }
//
//         [Test]
//         public void Add_ValueIsByteArray_WhenShort_Type()
//         {
//             var value = new byte[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.ByteVector));
//         }
//
//         [Test]
//         public void Add_ValueIsByteArray_WhenShort_Format()
//         {
//             var value = new byte[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 1));
//         }
//
//         [Test]
//         public void Add_ValueIsByteArray_WhenLong_Type()
//         {
//             var value = Enumerable.Repeat((byte) 0, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.ByteArray));
//         }
//
//         [Test]
//         public void Add_ValueIsByteArray_WhenLong_Format()
//         {
//             var value = Enumerable.Repeat((byte) 0, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + 256));
//         }
//
//         [Test]
//         public void Add_ValueIsInt16Array_WhenShort_Type()
//         {
//             var value = new short[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.ShortVector));
//         }
//
//         [Test]
//         public void Add_ValueIsInt16Array_WhenShort_Format()
//         {
//             var value = new short[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 2));
//         }
//
//         [Test]
//         public void Add_ValueIsInt16Array_WhenLong_Type()
//         {
//             var value = Enumerable.Repeat((short) 0, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.ShortArray));
//         }
//
//         [Test]
//         public void Add_ValueIsInt16Array_WhenLong_Format()
//         {
//             var value = Enumerable.Repeat((short) 0, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + 2 * 256));
//         }
//
//         [Test]
//         public void Add_ValueIsInt32Array_WhenShort_Type()
//         {
//             var value = new[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.IntegerVector));
//         }
//
//         [Test]
//         public void Add_ValueIsInt32Array_WhenShort_Format()
//         {
//             var value = new[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 4));
//         }
//
//         [Test]
//         public void Add_ValueIsInt32Array_WhenLong_Type()
//         {
//             var value = Enumerable.Repeat(0, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.IntegerArray));
//         }
//
//         [Test]
//         public void Add_ValueIsInt32Array_WhenLong_Format()
//         {
//             var value = Enumerable.Repeat(0, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + 4 * 256));
//         }
//
//         [Test]
//         public void Add_ValueIsInt64Array_WhenShort_Type()
//         {
//             var value = new long[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.LongVector));
//         }
//
//         [Test]
//         public void Add_ValueIsInt64Array_WhenShort_Format()
//         {
//             var value = new long[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 8));
//         }
//
//         [Test]
//         public void Add_ValueIsInt64Array_WhenLong_Type()
//         {
//             var value = Enumerable.Repeat(0L, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Vector));
//         }
//
//         [Test]
//         public void Add_ValueIsInt64Array_WhenLong_Format()
//         {
//             var value = Enumerable.Repeat(0L, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + 8 * 256));
//         }
//
//         [Test]
//         public void Add_ValueIsBoolArray_WhenShort_Type()
//         {
//             var value = new[] {true};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Vector));
//         }
//
//         [Test]
//         public void Add_ValueIsBoolArray_WhenShort_Format()
//         {
//             var value = new[] {true};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 1));
//         }
//
//         [Test]
//         public void Add_ValueIsBoolArray_WhenLong_Type()
//         {
//             var value = Enumerable.Repeat(true, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Flag));
//         }
//
//         [Test]
//         public void Add_ValueIsBoolArray_WhenLong_Format()
//         {
//             var value = Enumerable.Repeat(true, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + 256));
//         }
//
//         [Test]
//         public void Add_ValueIsFloatArray_WhenShort_Type()
//         {
//             var value = new float[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.Vector));
//         }
//
//         [Test]
//         public void Add_ValueIsFloatArray_WhenShort_Format()
//         {
//             var value = new float[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 4));
//         }
//
//         [Test]
//         public void Add_ValueIsFloatArray_WhenLong_Type()
//         {
//             var value = Enumerable.Repeat(0F, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.FloatArray));
//         }
//
//         [Test]
//         public void Add_ValueIsFloatArray_WhenLong_Format()
//         {
//             var value = Enumerable.Repeat(0F, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + 4 * 256));
//         }
//
//         [Test]
//         public void Add_ValueIsDoubleArray_WhenShort_Type()
//         {
//             var value = new double[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.DoubleVector));
//         }
//
//         [Test]
//         public void Add_ValueIsDoubleArray_WhenShort_Format()
//         {
//             var value = new double[] {0};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 8));
//         }
//
//         [Test]
//         public void Add_ValueIsDoubleArray_WhenLong_Type()
//         {
//             var value = Enumerable.Repeat(0D, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.DoubleArray));
//         }
//
//         [Test]
//         public void Add_ValueIsDoubleArray_WhenLong_Format()
//         {
//             var value = Enumerable.Repeat(0D, 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + 8 * 256));
//         }
//
//         [Test]
//         public void Add_ValueIsStringArray_WhenShort_WhenContainsShort_Type()
//         {
//             var value = new[] {" "};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.StringVector));
//         }
//
//         [Test]
//         public void Add_ValueIsStringArray_WhenShort_WhenContainsShort_Format()
//         {
//             var value = new[] {" "};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 1 + 1));
//         }
//
//         [Test]
//         public void Add_ValueIsStringArray_WhenShort_WhenContainsLong_Type()
//         {
//             var value = new[] {new string(' ', 256)};
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.TextVector));
//         }
//
//         [Test]
//         public void Add_ValueIsStringArray_WhenShort_WhenContainsLong_Format()
//         {
//             var value = new[] { new string(' ', 256) };
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 1 + 4 + 256));
//         }
//
//         [Test]
//         public void Add_ValueIsStringArray_WhenLong_WhenContainsShort_Type()
//         {
//             var value = Enumerable.Repeat(" ", 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.StringArray));
//         }
//
//         [Test]
//         public void Add_ValueIsStringArray_WhenLong_WhenContainsShort_Format()
//         {
//             var value = Enumerable.Repeat(" ", 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + (1 + 1) * 256));
//         }
//
//         [Test]
//         public void Add_ValueIsStringArray_WhenLong_WhenContainsLong_Type()
//         {
//             var value = Enumerable.Repeat(new string(' ', 256), 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That((TagValueTypeDefinition)writer.Buffer[1 + 3], Is.EqualTo(TagValueTypeDefinition.TextArray));
//         }
//
//         [Test]
//         public void Add_ValueIsStringArray_WhenLong_WhenContainsLong_Format()
//         {
//             var value = Enumerable.Repeat(new string(' ', 256), 256).ToArray();
//
//             var writer = CreateWriter();
//             var builder = CreateBuilder(writer);
//
//             builder.AddVector("key", value);
//
//             Assert.That(writer.Position, Is.EqualTo(1 + 3 + 1 + 4 + (4 + 256) * 256));
//         }
//
//         private static HerculesBinaryWriter CreateWriter() => new HerculesBinaryWriter(0);
//
//         private static HerculesRecordPayloadBuilder CreateBuilder(IHerculesBinaryWriter writer) => new HerculesRecordPayloadBuilder(writer);
//     }
// }