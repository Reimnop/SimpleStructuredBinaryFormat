using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleStructuredBinaryFormat;

/// <summary>
/// Provides methods to write SSBF data to a stream.
/// </summary>
public static class SsbfWrite
{
    /// <summary>
    /// Writes the specified SSBF node to the stream.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="node">The node to write.</param>
    /// <param name="compression">The compression mode to use.</param>
    public static void WriteToStream(Stream stream, SsbfNode node, Compression compression = Compression.None)
    {
        // Write the magic number
        stream.Write(SsbfGlobal.MagicNumber);
        
        // Write the compression mode
        stream.WriteByte((byte)compression);
        
        // Write root node
        var dataStream = GetCompressionStream(stream, compression);
        WriteNode(dataStream, node);
        
        // Make sure to flush the stream
        dataStream.Flush();
    }

    private static void WriteNode(Stream stream, SsbfNode node)
    {
        stream.WriteByte((byte)node.Type);
        switch (node.Type)
        {
            case NodeType.Null:
                break;
            case NodeType.Object:
                WriteObject(stream, (SsbfObject)node);
                break;
            case NodeType.Array:
                WriteArray(stream, (SsbfArray)node);
                break;
            case NodeType.Boolean:
                WriteBoolean(stream, ((SsbfBooleanValue)node).Value);
                break;
            case NodeType.SByte:
                stream.Write(((SsbfSByteValue)node).Value);
                break;
            case NodeType.Short:
                stream.Write(((SsbfShortValue)node).Value);
                break;
            case NodeType.Integer:
                stream.Write(((SsbfIntegerValue)node).Value);
                break;
            case NodeType.Long:
                stream.Write(((SsbfLongValue)node).Value);
                break;
            case NodeType.Byte:
                stream.Write(((SsbfByteValue)node).Value);
                break;
            case NodeType.UShort:
                stream.Write(((SsbfUShortValue)node).Value);
                break;
            case NodeType.UInteger:
                stream.Write(((SsbfUIntegerValue)node).Value);
                break;
            case NodeType.ULong:
                stream.Write(((SsbfULongValue)node).Value);
                break;
            case NodeType.HalfFloat:
                stream.Write(((SsbfHalfFloatValue)node).Value);
                break;
            case NodeType.Single:
                stream.Write(((SsbfSingleValue)node).Value);
                break;
            case NodeType.Double:
                stream.Write(((SsbfDoubleValue)node).Value);
                break;
            case NodeType.String:
                WriteString(stream, ((SsbfStringValue)node).Value);
                break;
            case NodeType.ByteArray:
                WriteByteArray(stream, ((SsbfByteArray)node).Data);
                break;
            default:
                throw new NotSupportedException($"Node type '{node.Type}' is not supported");
        }
    }
    
    private static void WriteObject(Stream stream, SsbfObject node)
    {
        stream.Write(node.Count);
        foreach (var (key, value) in node)
        {
            WriteString(stream, key);
            WriteNode(stream, value);
        }
    }
    
    private static void WriteArray(Stream stream, SsbfArray node)
    {
        stream.Write(node.Count);
        foreach (var value in node)
            WriteNode(stream, value);
    }
    
    private static void WriteBoolean(Stream stream, bool value)
    {
        stream.WriteByte((byte)(value ? 1 : 0));
    }

    private static void WriteString(Stream stream, string value)
    {
        var buffer = Encoding.UTF8.GetBytes(value);
        stream.Write(buffer.Length);
        stream.Write(buffer);
    }
    
    private static void WriteByteArray(Stream stream, byte[] value)
    {
        stream.Write(value.Length);
        stream.Write(value);
    }
    
    private static void Write<T>(this Stream stream, T value) where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buffer), value);
        stream.Write(buffer);
    }
    
    private static Stream GetCompressionStream(Stream stream, Compression compression)
        => compression switch
        {
            Compression.None => stream,
            Compression.Gzip => new GZipStream(stream, CompressionMode.Compress, true),
            Compression.Deflate => new DeflateStream(stream, CompressionMode.Compress, true),
            _ => throw new NotSupportedException($"Compression mode '{compression}' is not supported.")
        };
}