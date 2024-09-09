using System.Globalization;

namespace SimpleStructuredBinaryFormat;

/// <summary>
/// Represents a primitive value.
/// </summary>
/// <typeparam name="T">The primitive value type.</typeparam>
public abstract class SsbfValue<T> : SsbfNode
{
    /// <summary>
    /// The value of the node.
    /// </summary>
    public abstract T Value { get; set; }
}

/// <summary>
/// Represents a boolean value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfBooleanValue(bool value) : SsbfValue<bool>
{
    public override NodeType Type => NodeType.Boolean;
    public override bool Value { get; set; } = value;
    
    public override string ToString() => Value ? "true" : "false";
}

/// <summary>
/// Represents a signed byte value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfSByteValue(sbyte value) : SsbfValue<sbyte>
{
    public override NodeType Type => NodeType.SByte;
    public override sbyte Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents a short value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfShortValue(short value) : SsbfValue<short>
{
    public override NodeType Type => NodeType.Short;
    public override short Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents an integer value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfIntegerValue(int value) : SsbfValue<int>
{
    public override NodeType Type => NodeType.Integer;
    public override int Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents a long value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfLongValue(long value) : SsbfValue<long>
{
    public override NodeType Type => NodeType.Long;
    public override long Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents a byte value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfByteValue(byte value) : SsbfValue<byte>
{
    public override NodeType Type => NodeType.Byte;
    public override byte Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents an unsigned short value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfUShortValue(ushort value) : SsbfValue<ushort>
{
    public override NodeType Type => NodeType.UShort;
    public override ushort Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents an unsigned integer value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfUIntegerValue(uint value) : SsbfValue<uint>
{
    public override NodeType Type => NodeType.UInteger;
    public override uint Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents an unsigned long value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfULongValue(ulong value) : SsbfValue<ulong>
{
    public override NodeType Type => NodeType.ULong;
    public override ulong Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents a half-precision floating-point value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfHalfFloatValue(Half value) : SsbfValue<Half>
{
    public override NodeType Type => NodeType.HalfFloat;
    public override Half Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents a single-precision floating-point value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfSingleValue(float value) : SsbfValue<float>
{
    public override NodeType Type => NodeType.Single;
    public override float Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents a double-precision floating-point value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfDoubleValue(double value) : SsbfValue<double>
{
    public override NodeType Type => NodeType.Double;
    public override double Value { get; set; } = value;
    
    public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
}

/// <summary>
/// Represents a string value.
/// </summary>
/// <param name="value">The value.</param>
public class SsbfStringValue(string value) : SsbfValue<string>
{
    public override NodeType Type => NodeType.String;
    public override string Value { get; set; } = value;

    public override string ToString() => $"\"{Value}\"";
}
