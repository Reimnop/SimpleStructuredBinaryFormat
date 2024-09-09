namespace SimpleStructuredBinaryFormat;

public enum Compression : byte
{
    None = 0,
    Gzip = 1,
    Deflate = 2,
    // Might support more in the future
}