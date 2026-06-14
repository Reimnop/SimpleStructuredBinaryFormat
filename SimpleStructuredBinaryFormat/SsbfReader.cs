using System.Buffers;
using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using BrotliStream = BrotliSharpLib.BrotliStream;

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
        /// <c>true</c> when inside an object and the next thing to read is a key string
        /// (followed immediately by a node-type byte).
        /// </summary>
        public bool ExpectingPropertyName { get; set; }
    }

    /// <summary>Whether the stream was Brotli-compressed.</summary>
    public bool UseCompression { get; private set; }

    /// <summary>The type of the token the reader is currently positioned on.</summary>
    public SsbfTokenType TokenType { get; private set; } = SsbfTokenType.None;

    // The original stream supplied to the constructor.
    private readonly Stream rootStream;

    // The stream that supplies node bytes — either rootStream or a BrotliStream over it.
    private Stream ActiveDataStream => compressionStream ?? rootStream;
    private BrotliStream? compressionStream;

    private readonly Stack<ScopeInfo> scopes = new();
    private readonly bool leaveOpen;

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
    }

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

        var stream = ActiveDataStream;

        // --- inside an object: read key string, then node-type byte ---
        if (scopes.TryPeek(out var topScope) && topScope.IsObject && topScope.ExpectingPropertyName)
        {
            // Read the key string unconditionally — empty keys are valid.
            var key = ReadStringPayload(stream);

            // Now read the node-type byte. If it is End (0x00) the key is a sentinel
            // and we discard it; the object is complete.
            var nodeTypeByte = ReadByte(stream);
            if (nodeTypeByte == (byte)NodeType.End)
            {
                scopes.Pop();
                TokenType = SsbfTokenType.EndObject;
                NotifyParentScopeValueConsumed();
                return true;
            }

            // Real key-node pair: emit the PropertyName token now, defer the value to the next Read().
            // We re-enter dispatch by falling through — but we already have the node-type byte, so
            // we handle it inline rather than reading another byte.
            currentPropertyName = key;
            topScope.ExpectingPropertyName = false;
            TokenType = SsbfTokenType.PropertyName;

            // Stash the node-type byte so the next Read() call picks it up without re-reading.
            _pendingNodeType = nodeTypeByte;
            return true;
        }

        // --- read the next node-type byte (array element, root value, or stashed value) ---
        byte nodeType;
        if (_pendingNodeType is byte pending)
        {
            _pendingNodeType = null;
            nodeType = pending;
        }
        else
        {
            var raw = stream.ReadByte();
            if (raw == -1)
                return false;
            nodeType = (byte)raw;
        }

        return DispatchNodeType(nodeType, stream);
    }

    // -------------------------------------------------------------------------
    // Value accessors
    // -------------------------------------------------------------------------

    /// <summary>Returns the property name of the current <see cref="SsbfTokenType.PropertyName"/> token.</summary>
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
    // Private — dispatch
    // -------------------------------------------------------------------------

    // When the object-scope branch reads the key and node-type byte together, it stashes
    // the node-type byte here so the *next* Read() call can dispatch without an extra stream read.
    private byte? _pendingNodeType;

    private bool DispatchNodeType(byte nodeType, Stream stream)
    {
        switch ((NodeType)nodeType)
        {
            case NodeType.End:
                // End node inside an array — arrays don't read a key first, so End appears
                // directly in the node-type position.
                if (scopes.TryPeek(out var arrScope) && !arrScope.IsObject)
                {
                    scopes.Pop();
                    TokenType = SsbfTokenType.EndArray;
                    NotifyParentScopeValueConsumed();
                    return true;
                }
                // End at root or in unexpected position — treat as end of stream.
                return false;

            case NodeType.Null:
                TokenType = SsbfTokenType.Null;
                break;

            case NodeType.Object:
                TokenType = SsbfTokenType.StartObject;
                scopes.Push(new ScopeInfo { IsObject = true, ExpectingPropertyName = true });
                return true; // End* token will call NotifyParentScopeValueConsumed

            case NodeType.Array:
                TokenType = SsbfTokenType.StartArray;
                scopes.Push(new ScopeInfo { IsObject = false, ExpectingPropertyName = false });
                return true;

            case NodeType.Boolean:
                TokenType = SsbfTokenType.Boolean;
                currentValue = ReadByte(stream) != 0;
                break;

            case NodeType.SByte:
                TokenType = SsbfTokenType.SByte;
                currentValue = (sbyte)ReadByte(stream);
                break;

            case NodeType.Short:
                TokenType = SsbfTokenType.Short;
                currentValue = ReadPrimitive<short>(stream);
                break;

            case NodeType.Integer:
                TokenType = SsbfTokenType.Integer;
                currentValue = ReadPrimitive<int>(stream);
                break;

            case NodeType.Long:
                TokenType = SsbfTokenType.Long;
                currentValue = ReadPrimitive<long>(stream);
                break;

            case NodeType.Byte:
                TokenType = SsbfTokenType.Byte;
                currentValue = ReadByte(stream);
                break;

            case NodeType.UShort:
                TokenType = SsbfTokenType.UShort;
                currentValue = ReadPrimitive<ushort>(stream);
                break;

            case NodeType.UInteger:
                TokenType = SsbfTokenType.UInteger;
                currentValue = ReadPrimitive<uint>(stream);
                break;

            case NodeType.ULong:
                TokenType = SsbfTokenType.ULong;
                currentValue = ReadPrimitive<ulong>(stream);
                break;

            case NodeType.HalfFloat:
                TokenType = SsbfTokenType.HalfFloat;
                currentValue = ReadPrimitive<Half>(stream);
                break;

            case NodeType.Single:
                TokenType = SsbfTokenType.Single;
                currentValue = ReadPrimitive<float>(stream);
                break;

            case NodeType.Double:
                TokenType = SsbfTokenType.Double;
                currentValue = ReadPrimitive<double>(stream);
                break;

            case NodeType.String:
                TokenType = SsbfTokenType.String;
                currentValue = ReadStringPayload(stream);
                break;

            case NodeType.ByteArray:
                TokenType = SsbfTokenType.ByteArray;
                var length = ReadPrimitive<int>(stream);
                var data = new byte[length];
                stream.ReadExactly(data);
                currentValue = data;
                break;

            default:
                throw new InvalidDataException($"Unknown node type byte: 0x{nodeType:X2}");
        }

        // Stop after root value fully consumed
        if (scopes.Count == 0 && TokenType != SsbfTokenType.None)
        {
            NotifyParentScopeValueConsumed(); // no-op at root, but consistent
            return true;
        }

        NotifyParentScopeValueConsumed();
        return true;
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

    private static byte ReadByte(Stream stream)
    {
        var b = stream.ReadByte();
        if (b == -1)
            throw new EndOfStreamException("Unexpected end of stream");
        return (byte)b;
    }

    private static T ReadPrimitive<T>(Stream stream) where T : unmanaged
    {
        Span<byte> buf = stackalloc byte[Unsafe.SizeOf<T>()];
        stream.ReadExactly(buf);
        return Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buf));
    }

    private static string ReadStringPayload(Stream stream)
    {
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
}
