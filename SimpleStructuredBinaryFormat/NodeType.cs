namespace SimpleStructuredBinaryFormat;

public enum NodeType : byte
{
    Null = 0,
    Object = 1,
    Array = 2,
    Boolean = 3,
    SByte = 4,
    Short = 5,
    Integer = 6,
    Long = 7,
    Byte = 8,
    UShort = 9,
    UInteger = 10,
    ULong = 11,
    HalfFloat = 12,
    Single = 13,
    Double = 14,
    String = 15,
    ByteArray = 16,
}