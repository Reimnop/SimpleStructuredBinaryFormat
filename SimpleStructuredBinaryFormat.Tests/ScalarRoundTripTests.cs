namespace SimpleStructuredBinaryFormat.Tests;

public class ScalarRoundTripTests
{
    [Fact]
    public void Null_RoundTrips()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteNull());
        TestHelpers.AssertRead(reader, SsbfTokenType.Null);
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Boolean_RoundTrips(bool value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteBoolean(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.Boolean);
        Assert.Equal(value, reader.GetBoolean());
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData((sbyte)0)]
    [InlineData((sbyte)127)]
    [InlineData((sbyte)-128)]
    public void SByte_RoundTrips(sbyte value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteSByte(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.SByte);
        Assert.Equal(value, reader.GetSByte());
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData((short)0)]
    [InlineData((short)32767)]
    [InlineData((short)-32768)]
    public void Short_RoundTrips(short value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteShort(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.Short);
        Assert.Equal(value, reader.GetShort());
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    [InlineData(12345678)]
    public void Integer_RoundTrips(int value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteInteger(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.Equal(value, reader.GetInteger());
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData(0L)]
    [InlineData(long.MaxValue)]
    [InlineData(long.MinValue)]
    [InlineData(9876543210L)]
    public void Long_RoundTrips(long value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteLong(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.Long);
        Assert.Equal(value, reader.GetLong());
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData((byte)0)]
    [InlineData((byte)255)]
    [InlineData((byte)128)]
    public void Byte_RoundTrips(byte value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteByte(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.Byte);
        Assert.Equal(value, reader.GetByte());
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData((ushort)0)]
    [InlineData((ushort)65535)]
    [InlineData((ushort)1000)]
    public void UShort_RoundTrips(ushort value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteUShort(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.UShort);
        Assert.Equal(value, reader.GetUShort());
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData(0u)]
    [InlineData(uint.MaxValue)]
    [InlineData(123456789u)]
    public void UInteger_RoundTrips(uint value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteUInteger(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.UInteger);
        Assert.Equal(value, reader.GetUInteger());
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData(0UL)]
    [InlineData(ulong.MaxValue)]
    [InlineData(9876543210UL)]
    public void ULong_RoundTrips(ulong value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteULong(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.ULong);
        Assert.Equal(value, reader.GetULong());
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void Single_RoundTrips()
    {
        const float value = 3.14f;
        using var reader = TestHelpers.WriteAndRead(w => w.WriteSingle(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.Single);
        Assert.Equal(value, reader.GetSingle());
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void Single_NaN_RoundTrips()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteSingle(float.NaN));
        TestHelpers.AssertRead(reader, SsbfTokenType.Single);
        Assert.True(float.IsNaN(reader.GetSingle()));
    }

    [Fact]
    public void Single_Infinity_RoundTrips()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteSingle(float.PositiveInfinity));
        TestHelpers.AssertRead(reader, SsbfTokenType.Single);
        Assert.True(float.IsPositiveInfinity(reader.GetSingle()));
    }

    [Fact]
    public void Double_RoundTrips()
    {
        const double value = 2.718281828459045;
        using var reader = TestHelpers.WriteAndRead(w => w.WriteDouble(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.Double);
        Assert.Equal(value, reader.GetDouble());
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void Half_RoundTrips()
    {
        var value = (Half)1.5;
        using var reader = TestHelpers.WriteAndRead(w => w.WriteHalf(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.HalfFloat);
        Assert.Equal(value, reader.GetHalf());
        TestHelpers.AssertEnd(reader);
    }

    [Theory]
    [InlineData("")]
    [InlineData("hello")]
    [InlineData("hello, world!")]
    [InlineData("unicode: \u00e9\u4e2d\u6587\U0001F600")]
    public void String_RoundTrips(string value)
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteString(value));
        TestHelpers.AssertRead(reader, SsbfTokenType.String);
        Assert.Equal(value, reader.GetString());
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void ByteArray_Empty_RoundTrips()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteByteArray([]));
        TestHelpers.AssertRead(reader, SsbfTokenType.ByteArray);
        Assert.Equal([], reader.GetByteArray());
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void ByteArray_RoundTrips()
    {
        byte[] data = [1, 2, 3, 255, 0, 128];
        using var reader = TestHelpers.WriteAndRead(w => w.WriteByteArray(data));
        TestHelpers.AssertRead(reader, SsbfTokenType.ByteArray);
        Assert.Equal(data, reader.GetByteArray());
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void ByteArray_Large_RoundTrips()
    {
        var data = Enumerable.Range(0, 1024).Select(i => (byte)(i % 256)).ToArray();
        using var reader = TestHelpers.WriteAndRead(w => w.WriteByteArray(data));
        TestHelpers.AssertRead(reader, SsbfTokenType.ByteArray);
        Assert.Equal(data, reader.GetByteArray());
    }
}
