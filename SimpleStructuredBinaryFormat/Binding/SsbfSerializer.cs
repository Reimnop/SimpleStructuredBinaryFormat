namespace SimpleStructuredBinaryFormat.Binding;

/// <summary>
/// Provides top-level methods for serializing and deserializing objects to and from SSBF streams.
/// </summary>
/// <remarks>
/// Both methods require that <typeparamref name="T"/> is annotated with
/// <see cref="SsbfSerializableAttribute"/> so that the source generator can emit
/// the required <see cref="ISsbfBinder{T}"/> implementation.
/// </remarks>
public static class SsbfSerializer
{
    /// <summary>
    /// Serializes <paramref name="value"/> as a complete SSBF document to <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">A type annotated with <see cref="SsbfSerializableAttribute"/>.</typeparam>
    /// <param name="stream">A writable stream.</param>
    /// <param name="value">The object to serialize.</param>
    /// <param name="binder">
    /// The source-generated binder. Pass <c>new SsbfBinder&lt;T&gt;()</c> or the singleton exposed
    /// by the generated code (e.g. <c>MyTypeSsbfBinder.Instance</c>).
    /// </param>
    /// <param name="useCompression">When <c>true</c>, the data section is Brotli-compressed.</param>
    /// <param name="leaveOpen">When <c>true</c>, <paramref name="stream"/> is left open after writing.</param>
    public static void Serialize<T>(
        Stream stream,
        T value,
        ISsbfBinder<T> binder,
        bool useCompression = false,
        bool leaveOpen = false)
    {
        using var writer = new SsbfWriter(stream, useCompression, leaveOpen);
        binder.Serialize(value, writer);
        writer.Flush();
    }

    /// <summary>
    /// Deserializes a <typeparamref name="T"/> from an SSBF <paramref name="stream"/>.
    /// </summary>
    /// <typeparam name="T">A type annotated with <see cref="SsbfSerializableAttribute"/>.</typeparam>
    /// <param name="stream">A readable stream positioned at the start of an SSBF document.</param>
    /// <param name="binder">The source-generated binder.</param>
    /// <param name="leaveOpen">When <c>true</c>, <paramref name="stream"/> is left open after reading.</param>
    public static T Deserialize<T>(
        Stream stream,
        ISsbfBinder<T> binder,
        bool leaveOpen = false)
    {
        using var reader = new SsbfReader(stream, leaveOpen);
        return binder.Deserialize(reader);
    }
}
