namespace SimpleStructuredBinaryFormat.Binding;

/// <summary>
/// Implemented by source-generated code for every <see cref="SsbfSerializableAttribute"/>-annotated type.
/// Use <see cref="SsbfSerializer"/> rather than calling these methods directly.
/// </summary>
/// <typeparam name="T">The type this binder handles.</typeparam>
public interface ISsbfBinder<T>
{
    /// <summary>Serializes <paramref name="value"/> by writing SSBF tokens to <paramref name="writer"/>.</summary>
    void Serialize(T value, SsbfWriter writer);

    /// <summary>Deserializes a value by reading SSBF tokens from <paramref name="reader"/>.</summary>
    T Deserialize(SsbfReader reader);
}
