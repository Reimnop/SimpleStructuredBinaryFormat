namespace SimpleStructuredBinaryFormat.Binding;

/// <summary>
/// Excludes a field or property from SSBF serialization and deserialization.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SsbfIgnoreAttribute : Attribute;
