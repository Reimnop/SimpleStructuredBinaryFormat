namespace SimpleStructuredBinaryFormat;

/// <summary>
/// Describes the type of token the <see cref="SsbfReader"/> is currently positioned on.
/// </summary>
public enum SsbfTokenType
{
    /// <summary>No token has been read yet.</summary>
    None,

    /// <summary>The start of an object (<c>{</c>).</summary>
    StartObject,

    /// <summary>The end of an object (<c>}</c>).</summary>
    EndObject,

    /// <summary>The start of an array (<c>[</c>).</summary>
    StartArray,

    /// <summary>The end of an array (<c>]</c>).</summary>
    EndArray,

    /// <summary>A property name inside an object.</summary>
    PropertyName,

    /// <summary>A null value.</summary>
    Null,

    /// <summary>A <see cref="bool"/> value.</summary>
    Boolean,

    /// <summary>An <see cref="sbyte"/> value.</summary>
    SByte,

    /// <summary>A <see cref="short"/> value.</summary>
    Short,

    /// <summary>An <see cref="int"/> value.</summary>
    Integer,

    /// <summary>A <see cref="long"/> value.</summary>
    Long,

    /// <summary>A <see cref="byte"/> value.</summary>
    Byte,

    /// <summary>A <see cref="ushort"/> value.</summary>
    UShort,

    /// <summary>A <see cref="uint"/> value.</summary>
    UInteger,

    /// <summary>A <see cref="ulong"/> value.</summary>
    ULong,

    /// <summary>A <see cref="Half"/> value.</summary>
    HalfFloat,

    /// <summary>A <see cref="float"/> value.</summary>
    Single,

    /// <summary>A <see cref="double"/> value.</summary>
    Double,

    /// <summary>A <see cref="string"/> value.</summary>
    String,

    /// <summary>A byte-array value.</summary>
    ByteArray
}
