namespace SimpleStructuredBinaryFormat;

public enum NodeType : byte
{
    End = 0,
    Null = 1,
    Object = 2,
    Array = 3,
    Boolean = 4,
    SByte = 5,
    Short = 6,
    Integer = 7,
    Long = 8,
    Byte = 9,
    UShort = 10,
    UInteger = 11,
    ULong = 12,
    HalfFloat = 13,
    Single = 14,
    Double = 15,
    String = 16,
    ByteArray = 17
}