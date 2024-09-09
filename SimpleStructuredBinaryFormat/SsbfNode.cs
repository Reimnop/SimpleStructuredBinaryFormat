namespace SimpleStructuredBinaryFormat;

/// <summary>
/// The base class for all nodes in the Simple Structured Binary Format.
/// </summary>
public abstract class SsbfNode
{
    /// <summary>
    /// The type of the node.
    /// </summary>
    public abstract NodeType Type { get; }
    
    /// <summary>
    /// Gets or sets the element at index of this node, if this node is a <see cref="SsbfArray"/>
    /// </summary>
    /// <param name="index">The index of the element.</param>
    public SsbfNode this[int index]
    {
        get => ((SsbfArray)this)[index];
        set => ((SsbfArray)this)[index] = value;
    }
    
    /// <summary>
    /// Gets or sets the element with the specified key, if this node is a <see cref="SsbfObject"/>
    /// </summary>
    /// <param name="key">The key of the element.</param>
    public SsbfNode this[string key]
    {
        get => ((SsbfObject)this)[key];
        set => ((SsbfObject)this)[key] = value;
    }
    
    // Conversion
    public static implicit operator SsbfNode(bool value) => new SsbfBooleanValue(value);
    public static implicit operator SsbfNode(sbyte value) => new SsbfSByteValue(value);
    public static implicit operator SsbfNode(short value) => new SsbfShortValue(value);
    public static implicit operator SsbfNode(int value) => new SsbfIntegerValue(value);
    public static implicit operator SsbfNode(long value) => new SsbfLongValue(value);
    public static implicit operator SsbfNode(byte value) => new SsbfByteValue(value);
    public static implicit operator SsbfNode(ushort value) => new SsbfUShortValue(value);
    public static implicit operator SsbfNode(uint value) => new SsbfUIntegerValue(value);
    public static implicit operator SsbfNode(ulong value) => new SsbfULongValue(value);
    public static implicit operator SsbfNode(Half value) => new SsbfHalfFloatValue(value);
    public static implicit operator SsbfNode(float value) => new SsbfSingleValue(value);
    public static implicit operator SsbfNode(double value) => new SsbfDoubleValue(value);
    public static implicit operator SsbfNode(string value) => new SsbfStringValue(value);
    public static implicit operator SsbfNode(byte[] value) => new SsbfByteArray(value);
    public static implicit operator SsbfNode(ReadOnlySpan<byte> value) => new SsbfByteArray(value);
    
    public static explicit operator bool(SsbfNode node) => ((SsbfBooleanValue)node).Value;
    public static explicit operator sbyte(SsbfNode node) => ((SsbfSByteValue)node).Value;
    public static explicit operator short(SsbfNode node) => ((SsbfShortValue)node).Value;
    public static explicit operator int(SsbfNode node) => ((SsbfIntegerValue)node).Value;
    public static explicit operator long(SsbfNode node) => ((SsbfLongValue)node).Value;
    public static explicit operator byte(SsbfNode node) => ((SsbfByteValue)node).Value;
    public static explicit operator ushort(SsbfNode node) => ((SsbfUShortValue)node).Value;
    public static explicit operator uint(SsbfNode node) => ((SsbfUIntegerValue)node).Value;
    public static explicit operator ulong(SsbfNode node) => ((SsbfULongValue)node).Value;
    public static explicit operator Half(SsbfNode node) => ((SsbfHalfFloatValue)node).Value;
    public static explicit operator float(SsbfNode node) => ((SsbfSingleValue)node).Value;
    public static explicit operator double(SsbfNode node) => ((SsbfDoubleValue)node).Value;
    public static explicit operator string(SsbfNode node) => ((SsbfStringValue)node).Value;
    public static explicit operator byte[](SsbfNode node) => ((SsbfByteArray)node).Data;
}