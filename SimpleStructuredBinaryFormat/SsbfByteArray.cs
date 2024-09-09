namespace SimpleStructuredBinaryFormat;

public class SsbfByteArray(byte[] data) : SsbfNode
{
    public override NodeType Type => NodeType.ByteArray;
    public byte[] Data { get; set; } = data;
    
    public SsbfByteArray(ReadOnlySpan<byte> data) : this(data.ToArray())
    {
    }
    
    public override string ToString() => $"byte[{Data.Length}]";
}
