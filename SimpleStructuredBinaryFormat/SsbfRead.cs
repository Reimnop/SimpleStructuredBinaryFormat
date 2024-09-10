using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleStructuredBinaryFormat;

/// <summary>
/// Provides methods to read SSBF data from a stream.
/// </summary>
public static class SsbfRead
{
    /// <summary>
    /// Reads an SSBF node from the stream.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The read node.</returns>
    /// <exception cref="InvalidDataException">Thrown when the read magic number does not match <see cref="SsbfGlobal.MagicNumber"/></exception>
    public static SsbfNode? ReadFromStream(Stream stream)
    {
        // Check magic number
        var magicNumber = stream.Read<int>();
        if (magicNumber != SsbfGlobal.MagicNumber)
            throw new InvalidDataException($"Invalid magic number, expected '{SsbfGlobal.MagicNumber:X}', got '{magicNumber:X}'");
        
        // Read compression mode
        var compression = (Compression)stream.ReadByte();
        
        // Read root node
        var dataStream = GetCompressionStream(stream, compression);
        return ReadNode(dataStream);
    }

    private static SsbfNode? ReadNode(Stream stream)
    {
        var type = (NodeType)stream.ReadByte();
        return type switch
        {
            NodeType.Null => null,
            NodeType.Object => ReadObject(stream),
            NodeType.Array => ReadArray(stream),
            NodeType.Boolean => new SsbfBooleanValue(stream.Read<bool>()),
            NodeType.SByte => new SsbfSByteValue(stream.Read<sbyte>()),
            NodeType.Short => new SsbfShortValue(stream.Read<short>()),
            NodeType.Integer => new SsbfIntegerValue(stream.Read<int>()),
            NodeType.Long => new SsbfLongValue(stream.Read<long>()),
            NodeType.Byte => new SsbfByteValue(stream.Read<byte>()),
            NodeType.UShort => new SsbfUShortValue(stream.Read<ushort>()),
            NodeType.UInteger => new SsbfUIntegerValue(stream.Read<uint>()),
            NodeType.ULong => new SsbfULongValue(stream.Read<ulong>()),
            NodeType.Single => new SsbfSingleValue(stream.Read<float>()),
            NodeType.Double => new SsbfDoubleValue(stream.Read<double>()),
            NodeType.String => new SsbfStringValue(stream.ReadString()),
            NodeType.ByteArray => ReadByteArray(stream),
            _ => throw new NotSupportedException($"Node type '{type}' is not supported")
        };
    }
    
    private static SsbfObject ReadObject(Stream stream)
    {
        var count = stream.Read<int>();
        var obj = new SsbfObject(count);
        for (var i = 0; i < count; i++)
        {
            var key = stream.ReadString();
            var value = ReadNode(stream);
            obj[key] = value;
        }
        return obj;
    }
    
    private static SsbfArray ReadArray(Stream stream)
    {
        var count = stream.Read<int>();
        var array = new SsbfArray(count);
        for (var i = 0; i < count; i++)
            array[i] = ReadNode(stream);
        return array;
    }
    
    private static SsbfByteArray ReadByteArray(Stream stream)
    {
        var length = stream.Read<int>();
        var data = new byte[length];
        var offset = 0;
        var remaining = length;
        while (remaining > 0)
        {
            var read = stream.Read(data, offset, remaining);
            if (read == 0)
                throw new EndOfStreamException("End of stream reached before byte array could be fully read");
            offset += read;
            remaining -= read;
        }
        return new SsbfByteArray(data);
    }
    
    private static string ReadString(this Stream stream)
    {
        var length = stream.Read<int>();
        var buffer = new byte[length];
        var offset = 0;
        var remaining = length;
        while (remaining > 0)
        {
            var read = stream.Read(buffer, offset, remaining);
            if (read == 0)
                throw new EndOfStreamException("End of stream reached before string could be fully read");
            offset += read;
            remaining -= read;
        }
        return Encoding.UTF8.GetString(buffer);
    }

    private static Stream GetCompressionStream(Stream stream, Compression compression)
        => compression switch
        {
            Compression.None => stream,
            Compression.Gzip => new GZipStream(stream, CompressionMode.Decompress, true),
            Compression.Deflate => new DeflateStream(stream, CompressionMode.Decompress, true),
            _ => throw new NotSupportedException($"Compression mode '{compression}' is not supported.")
        };

    private static T Read<T>(this Stream stream) where T : unmanaged
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        var offset = 0;
        var remaining = buffer.Length;
        while (remaining > 0)
        {
            var read = stream.Read(buffer.Slice(offset, remaining));
            if (read == 0)
                throw new EndOfStreamException($"End of stream reached before '{typeof(T).Name}' could be fully read");
            offset += read;
            remaining -= read;
        }
        return MemoryMarshal.Read<T>(buffer);
    }
}