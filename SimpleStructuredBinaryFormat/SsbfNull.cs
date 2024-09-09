namespace SimpleStructuredBinaryFormat;

/// <summary>
/// Represents a null value.
/// </summary>
public class SsbfNull : SsbfNode
{
    public override NodeType Type => NodeType.Null;

    public override string ToString() => "null";
}