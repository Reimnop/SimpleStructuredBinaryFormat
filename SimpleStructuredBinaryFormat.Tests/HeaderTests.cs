namespace SimpleStructuredBinaryFormat.Tests;

public class HeaderTests
{
    [Fact]
    public void Reader_DetectsNoCompression()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteNull(), useCompression: false);
        reader.Read();
        Assert.False(reader.UseCompression);
    }

    [Fact]
    public void Reader_DetectsCompression()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteNull(), useCompression: true);
        reader.Read();
        Assert.True(reader.UseCompression);
    }

    [Fact]
    public void Reader_ThrowsOnBadMagicNumber()
    {
        var ms = new MemoryStream([0x00, 0x00, 0x00, 0x00, 0x00]); // wrong magic
        using var reader = new SsbfReader(ms);
        Assert.Throws<InvalidDataException>(() => reader.Read());
    }

    [Fact]
    public void Reader_ThrowsOnEmptyStream()
    {
        var ms = new MemoryStream([]);
        using var reader = new SsbfReader(ms);
        Assert.Throws<Exception>(() => reader.Read()); // EndOfStreamException
    }

    [Fact]
    public void TokenType_IsNoneBeforeFirstRead()
    {
        using var reader = TestHelpers.WriteAndRead(w => w.WriteNull());
        Assert.Equal(SsbfTokenType.None, reader.TokenType);
    }
}
