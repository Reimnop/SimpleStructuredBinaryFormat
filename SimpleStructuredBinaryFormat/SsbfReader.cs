using System.Buffers;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace SimpleStructuredBinaryFormat;

/// <summary>
/// A forward-only sequential reader for SSBF data.
/// <para>
/// Call <see cref="Read"/> to advance to the next token.
/// The current token type is exposed via <see cref="TokenType"/>.
/// Inside an object, each value token is preceded by a <see cref="SsbfTokenType.PropertyName"/> token
/// whose text is available via <see cref="GetPropertyName"/>.
/// Scalar values are retrieved with the typed <c>Get…()</c> methods.
/// </para>
/// </summary>
public class SsbfReader : IDisposable
{
    private sealed class ScopeInfo
    {
        public required bool IsObject { get; init; }

        /// <summary>
        /// <c>true</c> when the reader is positioned between two key-value pairs and
        /// the next byte should be interpreted as either a null terminator (end of scope)
        /// or the first byte of a property-name string.
        /// </summary>
        public bool ExpectingPropertyName { get; set; }
    }

    /// <summary>Whether the stream was Brotli-compressed.</summary>
    public bool UseCompression { get; private set; }

    /// <summary>The type of the token the reader is currently positioned on.</summary>
    public SsbfTokenType TokenType { get; private set; } = SsbfTokenType.None;

    // The stream that supplies node bytes. May be a BrotliStream wrapping rootStream.
    private readonly Stream dataStream;

    // The original stream supplied to the constructor.
    private readonly Stream rootStream;

    private readonly Stack<ScopeInfo> scopes = new();

    private readonly bool leaveOpen;

    // Cached value for the current token (lazy, set on demand).
    private string? currentPropertyName;
    private object? currentValue; // boxed scalar or null

    private bool headerRead;
    private bool disposed;

    /// <summary>
    /// Creates a new <see cref="SsbfReader"/> that reads from <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">Source stream. Must be readable.</param>
    /// <param name="leaveOpen">
    /// When <c>true</c>, <paramref name="stream"/> is left open on <see cref="Dispose"/>.
    /// </param>
    public SsbfReader(Stream stream, bool leaveOpen = false)
    {
        this.leaveOpen = leaveOpen;
        rootStream = stream;

        // We can't set up dataStream until we've read the header, so temporarily point at rootStream.
        // ReadHeader() will replace it with a BrotliStream when compression is used.
        dataStream = rootStream; // placeholder; replaced in ReadHeader for compressed streams
    }

    // We use a separate field so the constructor can be simple.
    private Stream ActiveDataStream => compressionStream ?? rootStream;
    private BrotliStream? compressionStream;

    /// <summary>
    /// Advances the reader to the next token.
    /// </summary>
    /// <returns>
    /// <c>true</c> if a token was read; <c>false</c> when the root value has been fully consumed.
    /// </returns>
    public bool Read()
    {
        ThrowIfDisposed();
        EnsureHeader();

        currentPropertyName = null;
        currentValue = null;

        // --- inside an object: handle property-name / end-of-object ---
        if (scopes.TryPeek(out var topScope) && topScope.IsObject && topScope.ExpectingPropertyName)
        {
            // Peek at the next byte to distinguish null-terminator from key start.
            var peek = PeekByte(ActiveDataStream);

            if (peek == 0x00)
            {
                // Consume the terminator.
                ConsumeByte(ActiveDataStream);
                scopes.Pop();
                TokenType = SsbfTokenType.EndObject;
                NotifyParentScopeValueConsumed();
                return true;
            }

            // It's the first byte of a null-terminated key string.
            ConsumeByte(ActiveDataStream); // consume the peeked byte
            currentPropertyName = ReadStringPayloadWithFirstByte(ActiveDataStream, (byte)peek);
            topScope.ExpectingPropertyName = false;
            TokenType = SsbfTokenType.PropertyName;
            return true;
        }

        // --- inside an array: check for end-of-array terminator ---
        if (topScope is { IsObject: false })
        {
            var peek = PeekByte(ActiveDataStream);

            if (peek == 0x00)
            {
                ConsumeByte(ActiveDataStream);
                scopes.Pop();
                TokenType = SsbfTokenType.EndArray;
                NotifyParentScopeValueConsumed();
                return true;
            }
        }

        // --- root: nothing more to read after the root value was fully consumed ---
        if (scopes.Count == 0 && TokenType != SsbfTokenType.None
            && TokenType is not SsbfTokenType.StartObject and not SsbfTokenType.StartArray)
            return false;

        // --- read the next node type byte ---
        var nodeTypeByte = PeekByte(ActiveDataStream);
        if (nodeTypeByte == -1)
            return false;
        ConsumeByte(ActiveDataStream);

        var nodeType = (NodeType)(byte)nodeTypeByte;

        switch (nodeType)
        {
            case NodeType.Null:
                TokenType = SsbfTokenType.Null;
                currentValue = null;
                break;

            case NodeType.Object:
                TokenType = SsbfTokenType.StartObject;
                scopes.Push(new ScopeInfo { IsObject = true, ExpectingPropertyName = true });
                return true; // don't call NotifyParentScopeValueConsumed yet — End* does that

            case NodeType.Array:
                TokenType = SsbfTokenType.StartArray;
                scopes.Push(new ScopeInfo { IsObject = false, ExpectingPropertyName = false });
                return true;

            case NodeType.Boolean:
                TokenType = SsbfTokenType.Boolean;
                currentValue = ConsumeByte(ActiveDataStream) != 0;
                break;

            case NodeType.SByte:
                TokenType = SsbfTokenType.SByte;
                currentValue = (sbyte)ConsumeByte(ActiveDataStream);
                break;

            case NodeType.Short:
                TokenType = SsbfTokenType.Short;
                currentValue = ReadPrimitive<short>(ActiveDataStream);
                break;

            case NodeType.Integer:
                TokenType = SsbfTokenType.Integer;
                currentValue = ReadPrimitive<int>(ActiveDataStream);
                break;

            case NodeType.Long:
                TokenType = SsbfTokenType.Long;
                currentValue = ReadPrimitive<long>(ActiveDataStream);
                break;

            case NodeType.Byte:
                TokenType = SsbfTokenType.Byte;
                currentValue = ConsumeByte(ActiveDataStream);
                break;

            case NodeType.UShort:
                TokenType = SsbfTokenType.UShort;
                currentValue = ReadPrimitive<ushort>(ActiveDataStream);
                break;

            case NodeType.UInteger:
                TokenType = SsbfTokenType.UInteger;
                currentValue = ReadPrimitive<uint>(ActiveDataStream);
                break;

            case NodeType.ULong:
                TokenType = SsbfTokenType.ULong;
                currentValue = ReadPrimitive<ulong>(ActiveDataStream);
                break;

            case NodeType.HalfFloat:
                TokenType = SsbfTokenType.HalfFloat;
                currentValue = ReadPrimitive<Half>(ActiveDataStream);
                break;

            case NodeType.Single:
                TokenType = SsbfTokenType.Single;
                currentValue = ReadPrimitive<float>(ActiveDataStream);
                break;

            case NodeType.Double:
                TokenType = SsbfTokenType.Double;
                currentValue = ReadPrimitive<double>(ActiveDataStream);
                break;

            case NodeType.String:
                TokenType = SsbfTokenType.String;
                currentValue = ReadStringPayload(ActiveDataStream);
                break;

            case NodeType.ByteArray:
                TokenType = SsbfTokenType.ByteArray;
                var length = ReadPrimitive<int>(ActiveDataStream);
                var data = new byte[length];
                ActiveDataStream.ReadExactly(data);
                currentValue = data;
                break;

            default:
                throw new InvalidDataException($"Unknown node type byte: 0x{nodeTypeByte:X2}");
        }

        NotifyParentScopeValueConsumed();
        return true;
    }

    // -------------------------------------------------------------------------
    // Value accessors
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns the property name of the current <see cref="SsbfTokenType.PropertyName"/> token.
    /// </summary>
    public string GetPropertyName()
    {
        ThrowIfNotToken(SsbfTokenType.PropertyName);
        return currentPropertyName!;
    }

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.Boolean"/> token.</summary>
    public bool GetBoolean() => GetValue<bool>(SsbfTokenType.Boolean);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.SByte"/> token.</summary>
    public sbyte GetSByte() => GetValue<sbyte>(SsbfTokenType.SByte);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.Short"/> token.</summary>
    public short GetShort() => GetValue<short>(SsbfTokenType.Short);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.Integer"/> token.</summary>
    public int GetInteger() => GetValue<int>(SsbfTokenType.Integer);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.Long"/> token.</summary>
    public long GetLong() => GetValue<long>(SsbfTokenType.Long);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.Byte"/> token.</summary>
    public byte GetByte() => GetValue<byte>(SsbfTokenType.Byte);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.UShort"/> token.</summary>
    public ushort GetUShort() => GetValue<ushort>(SsbfTokenType.UShort);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.UInteger"/> token.</summary>
    public uint GetUInteger() => GetValue<uint>(SsbfTokenType.UInteger);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.ULong"/> token.</summary>
    public ulong GetULong() => GetValue<ulong>(SsbfTokenType.ULong);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.HalfFloat"/> token.</summary>
    public Half GetHalf() => GetValue<Half>(SsbfTokenType.HalfFloat);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.Single"/> token.</summary>
    public float GetSingle() => GetValue<float>(SsbfTokenType.Single);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.Double"/> token.</summary>
    public double GetDouble() => GetValue<double>(SsbfTokenType.Double);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.String"/> token.</summary>
    public string GetString() => GetValue<string>(SsbfTokenType.String);

    /// <summary>Returns the value of the current <see cref="SsbfTokenType.ByteArray"/> token.</summary>
    public byte[] GetByteArray() => GetValue<byte[]>(SsbfTokenType.ByteArray);

    // -------------------------------------------------------------------------
    // IDisposable
    // -------------------------------------------------------------------------

    /// <inheritdoc />
    public void Dispose()
    {
        if (disposed)
            return;
        disposed = true;

        compressionStream?.Dispose();

        if (!leaveOpen)
            rootStream.Dispose();
    }

    // -------------------------------------------------------------------------
    // Private helpers
    // -------------------------------------------------------------------------

    private void EnsureHeader()
    {
        if (headerRead)
            return;
        headerRead = true;

        Span<byte> magic = stackalloc byte[4];
        rootStream.ReadExactly(magic);
        var magicNumber = MemoryMarshal.Read<int>(magic);

        if (magicNumber != SsbfGlobal.MagicNumber)
            throw new InvalidDataException(
                $"Invalid magic number: expected 0x{SsbfGlobal.MagicNumber:X8}, got 0x{magicNumber:X8}");

        var compressionByte = rootStream.ReadByte();
        if (compressionByte == -1)
            throw new EndOfStreamException("Unexpected end of stream while reading header");

        UseCompression = compressionByte != 0;

        if (UseCompression)
            compressionStream = new BrotliStream(rootStream, CompressionMode.Decompress, leaveOpen: true);
    }

    /// <summary>
    /// After writing a scalar (or EndObject/EndArray), notify the enclosing object scope
    /// that it should now expect the next property name.
    /// </summary>
    private void NotifyParentScopeValueConsumed()
    {
        if (scopes.Count == 0)
            return;

        var parent = scopes.Peek();
        if (parent.IsObject)
            parent.ExpectingPropertyName = true;
    }

    private T GetValue<T>(SsbfTokenType expected)
    {
        ThrowIfNotToken(expected);
        return (T)currentValue!;
    }

    private void ThrowIfNotToken(SsbfTokenType expected)
    {
        if (TokenType != expected)
            throw new InvalidOperationException(
                $"Cannot read {expected} when current token is {TokenType}");
    }

    private void ThrowIfDisposed()
        => ObjectDisposedException.ThrowIf(disposed, this);

    // --- byte-level I/O ---

    /// <summary>
    /// Reads one byte, draining the look-ahead buffer first if populated.
    /// </summary>
    private byte ConsumeByte(Stream stream)
    {
        if (hasPeeked)
        {
            hasPeeked = false;
            if (peekedByte == -1)
                throw new EndOfStreamException("Unexpected end of stream");
            return (byte)peekedByte;
        }

        var b = stream.ReadByte();
        if (b == -1)
            throw new EndOfStreamException("Unexpected end of stream");
        return (byte)b;
    }

    /// <summary>
    /// Returns the next byte without consuming it. Only used on non-compressed streams
    /// or on streams where we maintain our own buffer; for <see cref="BrotliStream"/> we
    /// rely on a small look-ahead buffer.
    /// </summary>
    private int PeekByte(Stream stream)
    {
        // BrotliStream doesn't support seeking, so we maintain a 1-byte look-ahead.
        if (hasPeeked)
            return peekedByte;

        peekedByte = stream.ReadByte();
        hasPeeked = true;
        return peekedByte;
    }

    private bool hasPeeked;
    private int peekedByte;

    private static T ReadPrimitive<T>(Stream stream) where T : unmanaged
    {
        Span<byte> buf = stackalloc byte[Unsafe.SizeOf<T>()];
        stream.ReadExactly(buf);
        return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buf));
    }

    private static string ReadStringPayload(Stream stream)
    {
        // Read until null terminator, growing a rented buffer as needed.
        var rented = ArrayPool<byte>.Shared.Rent(64);
        var pos = 0;
        try
        {
            while (true)
            {
                var b = stream.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException("Unexpected end of stream inside string");
                if (b == 0x00)
                    break;

                if (pos == rented.Length)
                {
                    var grown = ArrayPool<byte>.Shared.Rent(rented.Length * 2);
                    rented.AsSpan(0, pos).CopyTo(grown);
                    ArrayPool<byte>.Shared.Return(rented);
                    rented = grown;
                }

                rented[pos++] = (byte)b;
            }

            return Encoding.UTF8.GetString(rented, 0, pos);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    /// <summary>
    /// Reads a null-terminated string from <paramref name="stream"/> where the first byte
    /// has already been consumed and is passed in as <paramref name="firstByte"/>.
    /// </summary>
    private static string ReadStringPayloadWithFirstByte(Stream stream, byte firstByte)
    {
        var rented = ArrayPool<byte>.Shared.Rent(64);
        var pos = 0;
        try
        {
            rented[pos++] = firstByte;

            while (true)
            {
                var b = stream.ReadByte();
                if (b == -1)
                    throw new EndOfStreamException("Unexpected end of stream inside string");
                if (b == 0x00)
                    break;

                if (pos == rented.Length)
                {
                    var grown = ArrayPool<byte>.Shared.Rent(rented.Length * 2);
                    rented.AsSpan(0, pos).CopyTo(grown);
                    ArrayPool<byte>.Shared.Return(rented);
                    rented = grown;
                }

                rented[pos++] = (byte)b;
            }

            return Encoding.UTF8.GetString(rented, 0, pos);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }
}
