namespace SimpleStructuredBinaryFormat.Tests;

public class CompressionTests
{
    [Fact]
    public void Scalar_RoundTrips_WithCompression()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteInteger(42), useCompression: true);
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.True(reader.UseCompression); // header is parsed on first Read()
        Assert.Equal(42, reader.GetInteger());
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void Object_RoundTrips_WithCompression()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("key");
            w.WriteString("value");
            w.WriteEndObject();
        }, useCompression: true);

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName);
        Assert.Equal("key", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.String);
        Assert.Equal("value", reader.GetString());
        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void Array_RoundTrips_WithCompression()
    {
        var data = Enumerable.Range(0, 100).Select(i => (byte)(i % 256)).ToArray();

        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartArray();
            foreach (var b in data)
                w.WriteByte(b);
            w.WriteEndArray();
        }, useCompression: true);

        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray);
        foreach (var expected in data)
        {
            TestHelpers.AssertRead(reader, SsbfTokenType.Byte);
            Assert.Equal(expected, reader.GetByte());
        }
        TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);
    }

    [Fact]
    public void CompressedOutput_IsSmallerThanUncompressedForRepetitiveData()
    {
        static MemoryStream Write(bool compress)
        {
            var ms = new MemoryStream();
            using var w = new SsbfWriter(ms, compress, leaveOpen: true);
            w.WriteStartArray();
            for (var i = 0; i < 200; i++)
                w.WriteString("the quick brown fox jumps over the lazy dog");
            w.WriteEndArray();
            w.Flush();
            return ms;
        }

        var uncompressed = Write(false).Length;
        var compressed   = Write(true).Length;

        Assert.True(compressed < uncompressed,
            $"Expected compressed ({compressed}) < uncompressed ({uncompressed})");
    }
}
