namespace SimpleStructuredBinaryFormat.Tests;

/// <summary>
/// Tests for <see cref="SsbfDocument.Load"/> and <see cref="SsbfDocument.Save"/>.
/// </summary>
public class DocumentTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Saves <paramref name="node"/> to a <see cref="MemoryStream"/>, rewinds it,
    /// and loads it back, returning the reconstructed root node.
    /// </summary>
    private static SsbfNode SaveAndLoad(SsbfNode node, bool useCompression = false)
    {
        var ms = new MemoryStream();
        SsbfDocument.Save(ms, node, useCompression, leaveOpen: true);
        ms.Position = 0;
        return SsbfDocument.Load(ms, leaveOpen: false);
    }

    // -----------------------------------------------------------------------
    // Scalar round-trips
    // -----------------------------------------------------------------------

    [Fact]
    public void Null_RoundTrips()
    {
        var result = SaveAndLoad(new SsbfNull());
        Assert.True(result is SsbfNull);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Boolean_RoundTrips(bool value)
    {
        var result = SaveAndLoad(new SsbfBooleanValue(value));
        Assert.Equal(value, ((SsbfBooleanValue)result).Value);
    }

    [Theory]
    [InlineData((sbyte)0)]
    [InlineData(sbyte.MinValue)]
    [InlineData(sbyte.MaxValue)]
    public void SByte_RoundTrips(sbyte value)
    {
        var result = SaveAndLoad(new SsbfSByteValue(value));
        Assert.Equal(value, ((SsbfSByteValue)result).Value);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    public void Short_RoundTrips(short value)
    {
        var result = SaveAndLoad(new SsbfShortValue(value));
        Assert.Equal(value, ((SsbfShortValue)result).Value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(12345678)]
    public void Integer_RoundTrips(int value)
    {
        var result = SaveAndLoad(new SsbfIntegerValue(value));
        Assert.Equal(value, ((SsbfIntegerValue)result).Value);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    [InlineData(9876543210L)]
    public void Long_RoundTrips(long value)
    {
        var result = SaveAndLoad(new SsbfLongValue(value));
        Assert.Equal(value, ((SsbfLongValue)result).Value);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData(byte.MaxValue)]
    [InlineData((byte)128)]
    public void Byte_RoundTrips(byte value)
    {
        var result = SaveAndLoad(new SsbfByteValue(value));
        Assert.Equal(value, ((SsbfByteValue)result).Value);
    }

    [Theory]
    [InlineData((ushort)0)]
    [InlineData(ushort.MaxValue)]
    [InlineData((ushort)1000)]
    public void UShort_RoundTrips(ushort value)
    {
        var result = SaveAndLoad(new SsbfUShortValue(value));
        Assert.Equal(value, ((SsbfUShortValue)result).Value);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(uint.MaxValue)]
    [InlineData(123456789u)]
    public void UInteger_RoundTrips(uint value)
    {
        var result = SaveAndLoad(new SsbfUIntegerValue(value));
        Assert.Equal(value, ((SsbfUIntegerValue)result).Value);
    }

    [Theory]
    [InlineData(0ul)]
    [InlineData(ulong.MaxValue)]
    [InlineData(9876543210ul)]
    public void ULong_RoundTrips(ulong value)
    {
        var result = SaveAndLoad(new SsbfULongValue(value));
        Assert.Equal(value, ((SsbfULongValue)result).Value);
    }

    [Fact]
    public void Single_RoundTrips()
    {
        var result = SaveAndLoad(new SsbfSingleValue(3.14f));
        Assert.Equal(3.14f, ((SsbfSingleValue)result).Value);
    }

    [Fact]
    public void Single_NaN_RoundTrips()
    {
        var result = SaveAndLoad(new SsbfSingleValue(float.NaN));
        Assert.True(float.IsNaN(((SsbfSingleValue)result).Value));
    }

    [Fact]
    public void Single_Infinity_RoundTrips()
    {
        var result = SaveAndLoad(new SsbfSingleValue(float.PositiveInfinity));
        Assert.True(float.IsPositiveInfinity(((SsbfSingleValue)result).Value));
    }

    [Fact]
    public void Double_RoundTrips()
    {
        var result = SaveAndLoad(new SsbfDoubleValue(2.718281828));
        Assert.Equal(2.718281828, ((SsbfDoubleValue)result).Value);
    }

    [Fact]
    public void Half_RoundTrips()
    {
        var value = (Half)1.5;
        var result = SaveAndLoad(new SsbfHalfFloatValue(value));
        Assert.Equal(value, ((SsbfHalfFloatValue)result).Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("hello, world!")]
    [InlineData("unicode: é中文😀")]
    public void String_RoundTrips(string value)
    {
        var result = SaveAndLoad(new SsbfStringValue(value));
        Assert.Equal(value, ((SsbfStringValue)result).Value);
    }

    [Fact]
    public void ByteArray_RoundTrips()
    {
        byte[] data = [0x01, 0x02, 0x03, 0xFF];
        var result = SaveAndLoad(new SsbfByteArray(data));
        Assert.Equal(data, ((SsbfByteArray)result).Data);
    }

    [Fact]
    public void ByteArray_Empty_RoundTrips()
    {
        var result = SaveAndLoad(new SsbfByteArray([]));
        Assert.Equal([], ((SsbfByteArray)result).Data);
    }

    // -----------------------------------------------------------------------
    // Object structure
    // -----------------------------------------------------------------------

    [Fact]
    public void EmptyObject_RoundTrips()
    {
        var result = (SsbfObject)SaveAndLoad(new SsbfObject());
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void FlatObject_RoundTrips()
    {
        var obj = new SsbfObject
        {
            { "name",   new SsbfStringValue("Alice") },
            { "age",    new SsbfIntegerValue(30) },
            { "active", new SsbfBooleanValue(true) },
        };

        var result = (SsbfObject)SaveAndLoad(obj);

        Assert.Equal(3, result.Count);
        Assert.Equal("Alice", ((SsbfStringValue)result["name"]!).Value);
        Assert.Equal(30,      ((SsbfIntegerValue)result["age"]!).Value);
        Assert.Equal(true,    ((SsbfBooleanValue)result["active"]!).Value);
    }

    [Fact]
    public void Object_WithNullValue_RoundTrips()
    {
        var obj = new SsbfObject
        {
            { "key", new SsbfNull() },
        };

        var result = (SsbfObject)SaveAndLoad(obj);
        Assert.True(result["key"] is SsbfNull);
    }

    [Fact]
    public void NestedObject_RoundTrips()
    {
        var inner = new SsbfObject { { "city", new SsbfStringValue("Wonderland") } };
        var outer = new SsbfObject { { "address", inner } };

        var result = (SsbfObject)SaveAndLoad(outer);
        var resultInner = (SsbfObject)result["address"]!;
        Assert.Equal("Wonderland", ((SsbfStringValue)resultInner["city"]!).Value);
    }

    // -----------------------------------------------------------------------
    // Array structure
    // -----------------------------------------------------------------------

    [Fact]
    public void EmptyArray_RoundTrips()
    {
        var result = (SsbfArray)SaveAndLoad(new SsbfArray());
        Assert.Equal(0, result.Count);
    }

    [Fact]
    public void FlatArray_RoundTrips()
    {
        var arr = new SsbfArray
        {
            new SsbfIntegerValue(1),
            new SsbfIntegerValue(2),
            new SsbfIntegerValue(3),
        };

        var result = (SsbfArray)SaveAndLoad(arr);

        Assert.Equal(3, result.Count);
        Assert.Equal(1, ((SsbfIntegerValue)result[0]!).Value);
        Assert.Equal(2, ((SsbfIntegerValue)result[1]!).Value);
        Assert.Equal(3, ((SsbfIntegerValue)result[2]!).Value);
    }

    [Fact]
    public void Array_MixedTypes_RoundTrips()
    {
        var arr = new SsbfArray
        {
            new SsbfNull(),
            new SsbfBooleanValue(true),
            new SsbfStringValue("hi"),
            new SsbfIntegerValue(42),
        };

        var result = (SsbfArray)SaveAndLoad(arr);

        Assert.Equal(4, result.Count);
        Assert.True(result[0] is SsbfNull);
        Assert.Equal(true,  ((SsbfBooleanValue)result[1]!).Value);
        Assert.Equal("hi",  ((SsbfStringValue)result[2]!).Value);
        Assert.Equal(42,    ((SsbfIntegerValue)result[3]!).Value);
    }

    [Fact]
    public void Array_OfObjects_RoundTrips()
    {
        var arr = new SsbfArray
        {
            new SsbfObject { { "x", new SsbfIntegerValue(1) } },
            new SsbfObject { { "x", new SsbfIntegerValue(2) } },
        };

        var result = (SsbfArray)SaveAndLoad(arr);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, ((SsbfIntegerValue)((SsbfObject)result[0]!)["x"]!).Value);
        Assert.Equal(2, ((SsbfIntegerValue)((SsbfObject)result[1]!)["x"]!).Value);
    }

    [Fact]
    public void DeeplyNested_RoundTrips()
    {
        // { "a": { "b": { "c": 99 } } }
        var doc =
            new SsbfObject { { "a",
            new SsbfObject { { "b",
            new SsbfObject { { "c",
            new SsbfIntegerValue(99) } } } } } };

        var r1 = (SsbfObject)SaveAndLoad(doc);
        var r2 = (SsbfObject)r1["a"]!;
        var r3 = (SsbfObject)r2["b"]!;
        Assert.Equal(99, ((SsbfIntegerValue)r3["c"]!).Value);
    }

    // -----------------------------------------------------------------------
    // Realistic document
    // -----------------------------------------------------------------------

    [Fact]
    public void PersonDocument_RoundTrips_Uncompressed()
    {
        var doc = BuildPersonDocument();
        VerifyPersonDocument((SsbfObject)SaveAndLoad(doc, useCompression: false));
    }

    [Fact]
    public void PersonDocument_RoundTrips_Compressed()
    {
        var doc = BuildPersonDocument();
        VerifyPersonDocument((SsbfObject)SaveAndLoad(doc, useCompression: true));
    }

    private static SsbfObject BuildPersonDocument() =>
        new SsbfObject
        {
            { "name",   new SsbfStringValue("Alice") },
            { "age",    new SsbfIntegerValue(30) },
            { "active", new SsbfBooleanValue(true) },
            { "score",  new SsbfDoubleValue(98.6) },
            { "tags",   new SsbfArray
                        {
                            new SsbfStringValue("admin"),
                            new SsbfStringValue("user"),
                        }
            },
            { "avatar",  new SsbfByteArray(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }) },
            { "address", new SsbfObject
                         {
                             { "city", new SsbfStringValue("Wonderland") },
                             { "zip",  new SsbfNull() },
                         }
            },
        };

    private static void VerifyPersonDocument(SsbfObject doc)
    {
        Assert.Equal("Alice", ((SsbfStringValue)doc["name"]!).Value);
        Assert.Equal(30,      ((SsbfIntegerValue)doc["age"]!).Value);
        Assert.Equal(true,    ((SsbfBooleanValue)doc["active"]!).Value);
        Assert.Equal(98.6,    ((SsbfDoubleValue)doc["score"]!).Value);

        var tags = (SsbfArray)doc["tags"]!;
        Assert.Equal(2,       tags.Count);
        Assert.Equal("admin", ((SsbfStringValue)tags[0]!).Value);
        Assert.Equal("user",  ((SsbfStringValue)tags[1]!).Value);

        Assert.Equal(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, ((SsbfByteArray)doc["avatar"]!).Data);

        var address = (SsbfObject)doc["address"]!;
        Assert.Equal("Wonderland", ((SsbfStringValue)address["city"]!).Value);
        Assert.True(address["zip"] is SsbfNull);
    }

    // -----------------------------------------------------------------------
    // All numeric types
    // -----------------------------------------------------------------------

    [Fact]
    public void AllNumericTypes_RoundTrip()
    {
        var doc = new SsbfObject
        {
            { "i8",  new SsbfSByteValue(sbyte.MinValue) },
            { "i16", new SsbfShortValue(short.MaxValue) },
            { "i32", new SsbfIntegerValue(-1_000_000) },
            { "i64", new SsbfLongValue(long.MinValue) },
            { "u8",  new SsbfByteValue(byte.MaxValue) },
            { "u16", new SsbfUShortValue(ushort.MaxValue) },
            { "u32", new SsbfUIntegerValue(uint.MaxValue) },
            { "u64", new SsbfULongValue(ulong.MaxValue) },
            { "f16", new SsbfHalfFloatValue((Half)(-1.0)) },
            { "f32", new SsbfSingleValue(float.Epsilon) },
            { "f64", new SsbfDoubleValue(double.MaxValue) },
        };

        var result = (SsbfObject)SaveAndLoad(doc);

        Assert.Equal(sbyte.MinValue,  ((SsbfSByteValue)result["i8"]!).Value);
        Assert.Equal(short.MaxValue,  ((SsbfShortValue)result["i16"]!).Value);
        Assert.Equal(-1_000_000,      ((SsbfIntegerValue)result["i32"]!).Value);
        Assert.Equal(long.MinValue,   ((SsbfLongValue)result["i64"]!).Value);
        Assert.Equal(byte.MaxValue,   ((SsbfByteValue)result["u8"]!).Value);
        Assert.Equal(ushort.MaxValue, ((SsbfUShortValue)result["u16"]!).Value);
        Assert.Equal(uint.MaxValue,   ((SsbfUIntegerValue)result["u32"]!).Value);
        Assert.Equal(ulong.MaxValue,  ((SsbfULongValue)result["u64"]!).Value);
        Assert.Equal((Half)(-1.0),    ((SsbfHalfFloatValue)result["f16"]!).Value);
        Assert.Equal(float.Epsilon,   ((SsbfSingleValue)result["f32"]!).Value);
        Assert.Equal(double.MaxValue, ((SsbfDoubleValue)result["f64"]!).Value);
    }

    // -----------------------------------------------------------------------
    // LeaveOpen
    // -----------------------------------------------------------------------

    [Fact]
    public void Load_LeaveOpen_True_StreamRemainsOpen()
    {
        var ms = new MemoryStream();
        SsbfDocument.Save(ms, new SsbfIntegerValue(7), leaveOpen: true);
        ms.Position = 0;

        SsbfDocument.Load(ms, leaveOpen: true);

        Assert.True(ms.CanRead, "Stream should still be open after Load with leaveOpen: true");
    }

    [Fact]
    public void Save_LeaveOpen_True_StreamRemainsOpen()
    {
        var ms = new MemoryStream();
        SsbfDocument.Save(ms, new SsbfIntegerValue(7), leaveOpen: true);

        Assert.True(ms.CanWrite, "Stream should still be open after Save with leaveOpen: true");
    }

    // -----------------------------------------------------------------------
    // Save produces bytes readable by SsbfReader (and vice-versa)
    // -----------------------------------------------------------------------

    [Fact]
    public void Save_OutputIsReadableByRawReader()
    {
        var ms = new MemoryStream();
        SsbfDocument.Save(ms, new SsbfObject
        {
            { "x", new SsbfIntegerValue(42) },
        }, leaveOpen: true);

        ms.Position = 0;
        using var reader = new SsbfReader(ms);

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName);
        Assert.Equal("x", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.Equal(42, reader.GetInteger());
        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void Load_CanReadRawWriterOutput()
    {
        var ms = new MemoryStream();
        using (var writer = new SsbfWriter(ms, leaveOpen: true))
        {
            writer.WriteStartObject();
            writer.WritePropertyName("y");
            writer.WriteString("hello");
            writer.WriteEndObject();
            writer.Flush();
        }

        ms.Position = 0;
        var result = (SsbfObject)SsbfDocument.Load(ms);

        Assert.Equal("hello", ((SsbfStringValue)result["y"]!).Value);
    }
}
