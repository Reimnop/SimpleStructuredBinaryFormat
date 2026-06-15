namespace SimpleStructuredBinaryFormat.Binding;

/// <summary>
/// Overrides the SSBF property key used when serializing or deserializing a field or property.
/// When omitted the member's declared name is used as-is.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public sealed class SsbfPropertyAttribute(string name) : Attribute
{
    /// <summary>The key written to / expected from the SSBF object.</summary>
    public string Name { get; } = name;
}
