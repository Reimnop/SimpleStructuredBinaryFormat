namespace SimpleStructuredBinaryFormat;

/// <summary>
/// Provides static methods for reading and writing complete SSBF node trees.
/// <para>
/// Use <see cref="Load"/> / <see cref="Save"/> to convert between a <see cref="Stream"/>
/// and the in-memory <see cref="SsbfNode"/> representation.
/// Internally the methods delegate to <see cref="SsbfReader"/> and <see cref="SsbfWriter"/>
/// so the full format (compression, all node types) is supported.
/// </para>
/// </summary>
public static class SsbfDocument
{
    // -------------------------------------------------------------------------
    // Reading
    // -------------------------------------------------------------------------

    /// <summary>
    /// Reads an SSBF document from <paramref name="stream"/> and returns the root node.
    /// </summary>
    /// <param name="stream">A readable stream positioned at the start of an SSBF document.</param>
    /// <param name="leaveOpen">
    /// When <c>true</c>, <paramref name="stream"/> is left open after reading.
    /// </param>
    public static SsbfNode Load(Stream stream, bool leaveOpen = false)
    {
        using var reader = new SsbfReader(stream, leaveOpen: leaveOpen);
        return ReadNode(reader);
    }

    // -------------------------------------------------------------------------
    // Writing
    // -------------------------------------------------------------------------

    /// <summary>
    /// Writes <paramref name="node"/> as a complete SSBF document to <paramref name="stream"/>.
    /// </summary>
    /// <param name="stream">A writable stream.</param>
    /// <param name="node">The root node to write.</param>
    /// <param name="useCompression">When <c>true</c>, the data section is Brotli-compressed.</param>
    /// <param name="leaveOpen">
    /// When <c>true</c>, <paramref name="stream"/> is left open after writing.
    /// </param>
    public static void Save(Stream stream, SsbfNode node, bool useCompression = false, bool leaveOpen = false)
    {
        using var writer = new SsbfWriter(stream, useCompression: useCompression, leaveOpen: leaveOpen);
        WriteNode(writer, node);
        writer.Flush();
    }

    // -------------------------------------------------------------------------
    // Private — reading helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Advances <paramref name="reader"/> to the next token and returns the fully-parsed node.
    /// The reader must not have been exhausted.
    /// </summary>
    private static SsbfNode ReadNode(SsbfReader reader)
    {
        if (!reader.Read())
            throw new InvalidDataException("Unexpected end of SSBF data");

        return ReadCurrentNode(reader);
    }

    /// <summary>
    /// Parses the node that the reader is currently positioned on.
    /// For <see cref="SsbfTokenType.StartObject"/> and <see cref="SsbfTokenType.StartArray"/>
    /// this recursively reads all children until the matching End token.
    /// </summary>
    private static SsbfNode ReadCurrentNode(SsbfReader reader)
    {
        switch (reader.TokenType)
        {
            case SsbfTokenType.Null:
                return new SsbfNull();

            case SsbfTokenType.Boolean:
                return new SsbfBooleanValue(reader.GetBoolean());

            case SsbfTokenType.SByte:
                return new SsbfSByteValue(reader.GetSByte());

            case SsbfTokenType.Short:
                return new SsbfShortValue(reader.GetShort());

            case SsbfTokenType.Integer:
                return new SsbfIntegerValue(reader.GetInteger());

            case SsbfTokenType.Long:
                return new SsbfLongValue(reader.GetLong());

            case SsbfTokenType.Byte:
                return new SsbfByteValue(reader.GetByte());

            case SsbfTokenType.UShort:
                return new SsbfUShortValue(reader.GetUShort());

            case SsbfTokenType.UInteger:
                return new SsbfUIntegerValue(reader.GetUInteger());

            case SsbfTokenType.ULong:
                return new SsbfULongValue(reader.GetULong());

            case SsbfTokenType.HalfFloat:
                return new SsbfHalfFloatValue(reader.GetHalf());

            case SsbfTokenType.Single:
                return new SsbfSingleValue(reader.GetSingle());

            case SsbfTokenType.Double:
                return new SsbfDoubleValue(reader.GetDouble());

            case SsbfTokenType.String:
                return new SsbfStringValue(reader.GetString());

            case SsbfTokenType.ByteArray:
                return new SsbfByteArray(reader.GetByteArray());

            case SsbfTokenType.StartObject:
                return ReadObject(reader);

            case SsbfTokenType.StartArray:
                return ReadArray(reader);

            default:
                throw new InvalidDataException($"Unexpected token type: {reader.TokenType}");
        }
    }

    private static SsbfObject ReadObject(SsbfReader reader)
    {
        var obj = new SsbfObject();

        // Read tokens until we hit EndObject.
        while (reader.Read())
        {
            if (reader.TokenType == SsbfTokenType.EndObject)
                return obj;

            if (reader.TokenType != SsbfTokenType.PropertyName)
                throw new InvalidDataException($"Expected PropertyName token, got {reader.TokenType}");

            var key = reader.GetPropertyName();

            // The value node follows immediately.
            var value = ReadNode(reader);
            obj.Add(key, value);
        }

        throw new InvalidDataException("Unexpected end of SSBF data while reading object");
    }

    private static SsbfArray ReadArray(SsbfReader reader)
    {
        var array = new SsbfArray();

        // Read tokens until we hit EndArray.
        while (reader.Read())
        {
            if (reader.TokenType == SsbfTokenType.EndArray)
                return array;

            array.Add(ReadCurrentNode(reader));
        }

        throw new InvalidDataException("Unexpected end of SSBF data while reading array");
    }

    // -------------------------------------------------------------------------
    // Private — writing helpers
    // -------------------------------------------------------------------------

    private static void WriteNode(SsbfWriter writer, SsbfNode node)
    {
        switch (node)
        {
            case SsbfNull:
                writer.WriteNull();
                break;

            case SsbfBooleanValue v:
                writer.WriteBoolean(v.Value);
                break;

            case SsbfSByteValue v:
                writer.WriteSByte(v.Value);
                break;

            case SsbfShortValue v:
                writer.WriteShort(v.Value);
                break;

            case SsbfIntegerValue v:
                writer.WriteInteger(v.Value);
                break;

            case SsbfLongValue v:
                writer.WriteLong(v.Value);
                break;

            case SsbfByteValue v:
                writer.WriteByte(v.Value);
                break;

            case SsbfUShortValue v:
                writer.WriteUShort(v.Value);
                break;

            case SsbfUIntegerValue v:
                writer.WriteUInteger(v.Value);
                break;

            case SsbfULongValue v:
                writer.WriteULong(v.Value);
                break;

            case SsbfHalfFloatValue v:
                writer.WriteHalf(v.Value);
                break;

            case SsbfSingleValue v:
                writer.WriteSingle(v.Value);
                break;

            case SsbfDoubleValue v:
                writer.WriteDouble(v.Value);
                break;

            case SsbfStringValue v:
                writer.WriteString(v.Value);
                break;

            case SsbfByteArray v:
                writer.WriteByteArray(v.Data);
                break;

            case SsbfObject obj:
                writer.WriteStartObject();
                foreach (var (key, value) in obj)
                {
                    writer.WritePropertyName(key);
                    WriteNode(writer, value ?? new SsbfNull());
                }
                writer.WriteEndObject();
                break;

            case SsbfArray arr:
                writer.WriteStartArray();
                foreach (var item in arr)
                    WriteNode(writer, item ?? new SsbfNull());
                writer.WriteEndArray();
                break;

            default:
                throw new ArgumentException($"Unknown node type: {node.GetType().Name}", nameof(node));
        }
    }
}
