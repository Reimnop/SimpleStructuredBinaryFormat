namespace SimpleStructuredBinaryFormat.Tests;

public class ErrorHandlingTests
{
    // -----------------------------------------------------------------------
    // Wrong-token accessor calls
    // -----------------------------------------------------------------------

    [Fact]
    public void GetBoolean_ThrowsWhenNotOnBooleanToken()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteInteger(1));
        reader.Read();
        Assert.Throws<InvalidOperationException>(() => reader.GetBoolean());
    }

    [Fact]
    public void GetInteger_ThrowsWhenNotOnIntegerToken()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteString("hi"));
        reader.Read();
        Assert.Throws<InvalidOperationException>(() => reader.GetInteger());
    }

    [Fact]
    public void GetString_ThrowsWhenNotOnStringToken()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteNull());
        reader.Read();
        Assert.Throws<InvalidOperationException>(() => reader.GetString());
    }

    [Fact]
    public void GetPropertyName_ThrowsWhenNotOnPropertyNameToken()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("k");
            w.WriteNull();
            w.WriteEndObject();
        });

        reader.Read(); // StartObject
        Assert.Throws<InvalidOperationException>(() => reader.GetPropertyName());
    }

    [Fact]
    public void GetByteArray_ThrowsWhenNotOnByteArrayToken()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteBoolean(true));
        reader.Read();
        Assert.Throws<InvalidOperationException>(() => reader.GetByteArray());
    }

    // -----------------------------------------------------------------------
    // Disposed reader
    // -----------------------------------------------------------------------

    [Fact]
    public void Read_ThrowsAfterDispose()
    {
        var ms = new MemoryStream();
        using (var writer = new SsbfWriter(ms, leaveOpen: true))
        {
            writer.WriteNull();
            writer.Flush();
        }
        ms.Position = 0;

        var reader = new SsbfReader(ms);
        reader.Dispose();

        Assert.Throws<ObjectDisposedException>(() => reader.Read());
    }

    // -----------------------------------------------------------------------
    // End-of-stream cases
    // -----------------------------------------------------------------------

    [Fact]
    public void Read_ReturnsFalseWhenExhausted()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteNull());
        Assert.True(reader.Read());   // Null
        Assert.False(reader.Read());  // exhausted
        Assert.False(reader.Read());  // still exhausted
    }

    [Fact]
    public void Read_ReturnsFalseAfterEmptyObjectConsumed()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WriteEndObject();
        });

        Assert.True(reader.Read());   // StartObject
        Assert.True(reader.Read());   // EndObject
        Assert.False(reader.Read());  // exhausted
    }

    // -----------------------------------------------------------------------
    // Long property names (exercises the string buffer growth path)
    // -----------------------------------------------------------------------

    [Fact]
    public void LongPropertyName_RoundTrips()
    {
        var longKey = new string('a', 512);

        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName(longKey);
            w.WriteNull();
            w.WriteEndObject();
        });

        reader.Read(); // StartObject
        reader.Read(); // PropertyName
        Assert.Equal(longKey, reader.GetPropertyName());
    }

    [Fact]
    public void LongString_RoundTrips()
    {
        var longValue = new string('z', 1024);

        using var reader = TestHelpers.WriteAndRead(w => w.WriteString(longValue));
        TestHelpers.AssertRead(reader, SsbfTokenType.String);
        Assert.Equal(longValue, reader.GetString());
    }

    // -----------------------------------------------------------------------
    // Unicode strings
    // -----------------------------------------------------------------------

    [Fact]
    public void UnicodePropertyName_RoundTrips()
    {
        const string key = "\u4e2d\u6587\u30ad\u30fc";

        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName(key);
            w.WriteNull();
            w.WriteEndObject();
        });

        reader.Read(); // StartObject
        reader.Read(); // PropertyName
        Assert.Equal(key, reader.GetPropertyName());
    }
}
