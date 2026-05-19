namespace SimpleStructuredBinaryFormat.Binding;

/// <summary>
/// Marks a class or struct for SSBF source-generated serialization.
/// <para>
/// The source generator will emit an <see cref="ISsbfBinder{T}"/> implementation
/// for every type annotated with this attribute, allowing
/// <see cref="SsbfSerializer.Serialize{T}"/> and <see cref="SsbfSerializer.Deserialize{T}"/>
/// to work without reflection at runtime.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class SsbfSerializableAttribute : Attribute;
