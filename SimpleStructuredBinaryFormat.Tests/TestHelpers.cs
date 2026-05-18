namespace SimpleStructuredBinaryFormat.Tests;

/// <summary>
/// Helpers for round-trip testing: write with SsbfWriter, read back with SsbfReader.
/// </summary>
internal static class TestHelpers
{
    /// <summary>
    /// Executes <paramref name="write"/> against a fresh <see cref="SsbfWriter"/>,
    /// rewinds the buffer, and returns an <see cref="SsbfReader"/> positioned before
    /// the first token.
    /// </summary>
    public static SsbfReader WriteAndRead(Action<SsbfWriter> write, bool useCompression = false)
    {
        var ms = new MemoryStream();
        using (var writer = new SsbfWriter(ms, useCompression, leaveOpen: true))
        {
            write(writer);
            writer.Flush();
        }

        ms.Position = 0;
        return new SsbfReader(ms, leaveOpen: false);
    }

    /// <summary>
    /// Advances the reader by exactly one token and asserts its type.
    /// </summary>
    public static void AssertRead(SsbfReader reader, SsbfTokenType expected)
    {
        Assert.True(reader.Read(), $"Expected Read() to return true for {expected}");
        Assert.Equal(expected, reader.TokenType);
    }

    /// <summary>
    /// Asserts that the reader is exhausted (Read() returns false).
    /// </summary>
    public static void AssertEnd(SsbfReader reader)
    {
        Assert.False(reader.Read(), "Expected reader to be exhausted");
    }
}
