using System.Buffers;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleStructuredBinaryFormat;

/// <summary>
/// A forward-only sequential writer for SSBF data.
/// <para>
/// Call <see cref="WriteStartObject"/> / <see cref="WriteStartArray"/> to open scopes,
/// and <see cref="WriteEndObject"/> / <see cref="WriteEndArray"/> to close them.
/// Inside an object, every value must be preceded by <see cref="WritePropertyName"/>.
/// </para>
/// </summary>
public class SsbfWriter : IDisposable
{
    private sealed class ScopeInfo
    {
        public required bool IsObject { get; init; }
        public bool ExpectingPropertyName { get; set; }
    }
    
    /// <summary>Whether to wrap the output in a Brotli-compressed envelope.</summary>
    public bool UseCompression { get; }

    // The stream to write the root node and every child node into. May be compressed.
    private readonly Stream dataStream;
    
    // The original stream passed to the constructor.
    private readonly Stream rootStream;
    
    // Keeps track of the scopes that are currently open. The top of the stack is the current scope.
    private readonly Stack<ScopeInfo> scopes = new();
    
    private readonly bool leaveOpen;
    
    private bool headerWritten;
    private bool disposed;
    
    /// <summary>
    /// Creates a new <see cref="SsbfWriter"/> that writes to <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">Destination stream. Must be writable.</param>
    /// <param name="useCompression">When <c>true</c>, the data section is Brotli-compressed.</param>
    /// <param name="leaveOpen">
    /// When <c>true</c>, <paramref name="stream"/> is left open on <see cref="Dispose"/>.
    /// </param>
    public SsbfWriter(Stream stream, bool useCompression = false, bool leaveOpen = false)
    {
        UseCompression = useCompression;
        this.leaveOpen = leaveOpen;
        rootStream = stream;
        dataStream = useCompression 
            ? new BrotliStream(stream, CompressionMode.Compress, true)
            : stream;
    }
    
    /// <summary>
    /// Begins writing an object.
    /// </summary>
    public void WriteStartObject()
    {
        EnsureHeader();
        CheckValueAllowed();
        PushScope(true);
    }
    
    /// <summary>
    /// Ends the current object.
    /// </summary>
    public void WriteEndObject()
    {
        PopScope(true);
    }
    
    /// <summary>
    /// Begins writing an array.
    /// </summary>
    public void WriteStartArray()
    {
        EnsureHeader();
        CheckValueAllowed();
        PushScope(false);
    }
    
    /// <summary>
    /// Ends the current array.
    /// </summary>
    public void WriteEndArray()
    {
        PopScope(false);
    }
    
    /// <summary>
    /// Writes a property name inside an object.
    /// Must be called before each value when inside an object.
    /// </summary>
    public void WritePropertyName(string name)
    {
        ThrowIfDisposed();

        if (scopes.Count == 0 || !scopes.Peek().IsObject)
            throw new InvalidOperationException($"{nameof(WritePropertyName)} is only valid inside an object scope");

        var scope = scopes.Peek();
        if (!scope.ExpectingPropertyName)
            throw new InvalidOperationException("Expected value, but writing property name instead");

        WriteStringPayload(dataStream, name);
        scope.ExpectingPropertyName = false;
    }
    
    /// <summary>
    /// Writes a null value.
    /// </summary>
    public void WriteNull()
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.Null);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="bool"/> value.
    /// </summary>
    public void WriteBoolean(bool value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.Boolean);
        dataStream.WriteByte(value ? (byte)1 : (byte)0);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes an <see cref="sbyte"/> value.
    /// </summary>
    public void WriteSByte(sbyte value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.SByte);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="short"/> value.
    /// </summary>
    public void WriteShort(short value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.Short);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes an <see cref="int"/> value.
    /// </summary>
    public void WriteInteger(int value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.Integer);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="long"/> value.
    /// </summary>
    public void WriteLong(long value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.Long);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="byte"/> value.
    /// </summary>
    public void WriteByte(byte value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.Byte);
        dataStream.WriteByte(value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="ushort"/> value.
    /// </summary>
    public void WriteUShort(ushort value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.UShort);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="uint"/> value.
    /// </summary>
    public void WriteUInteger(uint value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.UInteger);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="ulong"/> value.
    /// </summary>
    public void WriteULong(ulong value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.ULong);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="Half"/> value.
    /// </summary>
    public void WriteHalf(Half value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.HalfFloat);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="float"/> value.
    /// </summary>
    public void WriteSingle(float value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.Single);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="double"/> value.
    /// </summary>
    public void WriteDouble(double value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.Double);
        WritePrimitive(dataStream, value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Writes a <see cref="string"/> value.
    /// </summary>
    public void WriteString(string value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.String);
        WriteStringPayload(dataStream, value);
        OnValueWritten();
    }

    /// <summary>
    /// Writes a byte-array value.
    /// </summary>
    public void WriteByteArray(ReadOnlySpan<byte> value)
    {
        EnsureHeader();
        CheckValueAllowed();
        dataStream.WriteByte((byte)NodeType.ByteArray);
        WritePrimitive(dataStream, value.Length);
        dataStream.Write(value);
        OnValueWritten();
    }
    
    /// <summary>
    /// Flushes all buffered data to the underlying stream.
    /// Throws if there are still open scopes.
    /// </summary>
    public void Flush()
    {
        ThrowIfDisposed();
        if (scopes.Count > 0)
            throw new InvalidOperationException("Cannot flush while there are still open scopes");
        dataStream.Flush();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed) 
            return;
        disposed = true;

        // Flush compression stream if applicable.
        if (UseCompression)
            dataStream.Dispose(); // BrotliStream writes its final block on Dispose

        if (!leaveOpen)
            rootStream.Dispose();
    }

    private void PushScope(bool isObject)
    {
        ThrowIfDisposed();
        
        dataStream.WriteByte(isObject ? (byte)NodeType.Object : (byte)NodeType.Array);
        scopes.Push(new ScopeInfo
        {
            IsObject = isObject, 
            ExpectingPropertyName = isObject
        });
    }

    private void PopScope(bool isObject)
    {
        ThrowIfDisposed();
        
        if (scopes.Count == 0)
            throw new InvalidOperationException(isObject 
                ? "No open object scope to end"
                : "No open array scope to end");
        
        var scope = scopes.Pop();
        
        if (scope.IsObject != isObject)
            throw new InvalidOperationException(
                isObject
                    ? "Cannot end the current object scope by ending an array scope"
                    : "Cannot end the current array scope by ending an object scope");
        
        if (scope.IsObject && !scope.ExpectingPropertyName)
            throw new InvalidOperationException($"Object is incomplete, expected value for property name");
        
        // write scope terminator
        dataStream.WriteByte(0);
        
        OnValueWritten();
    }

    private void OnValueWritten()
    {
        if (scopes.Count == 0) 
            return; // root value
        
        var scope = scopes.Peek();
        if (scope.IsObject)
            scope.ExpectingPropertyName = true;
    }
    
    private void CheckValueAllowed()
    {
        ThrowIfDisposed();

        if (scopes.Count == 0) 
            return; // root value

        var scope = scopes.Peek();
        if (scope is { IsObject: true, ExpectingPropertyName: true })
            throw new InvalidOperationException("Expected property name, but writing value instead");
    }

    private void EnsureHeader()
    {
        if (headerWritten)
            return;
        headerWritten = true;
        
        WritePrimitive(rootStream, SsbfGlobal.MagicNumber);
        rootStream.WriteByte(UseCompression ? (byte)1 : (byte)0);
    }
    
    private void ThrowIfDisposed() 
        => ObjectDisposedException.ThrowIf(disposed, this);
    
    private static void WriteStringPayload(Stream stream, string value)
    {
        // We encode first to get the byte length, then write length + bytes.
        var maxLen = Encoding.UTF8.GetMaxByteCount(value.Length);
        var rented = ArrayPool<byte>.Shared.Rent(maxLen);
        try
        {
            var written = Encoding.UTF8.GetBytes(value, rented);
            stream.Write(rented, 0, written);
            stream.WriteByte(0); // null terminator
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
    
    private static void WritePrimitive<T>(Stream stream, T value) where T : unmanaged
    {
        Span<byte> buf = stackalloc byte[Unsafe.SizeOf<T>()];
        Unsafe.WriteUnaligned(ref MemoryMarshal.GetReference(buf), value);
        stream.Write(buf);
    }
}