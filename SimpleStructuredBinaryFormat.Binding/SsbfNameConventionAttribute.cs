namespace SimpleStructuredBinaryFormat.Binding;

/// <summary>
/// Specifies the naming convention applied to all property keys when serializing or
/// deserializing a <see cref="SsbfSerializableAttribute"/>-annotated type.
/// <para>
/// Individual <see cref="SsbfPropertyAttribute"/> overrides always take priority over
/// the convention. <see cref="SsbfIgnoreAttribute"/> members are unaffected.
/// </para>
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
public sealed class SsbfNameConventionAttribute(NameConvention convention) : Attribute
{
    /// <summary>The convention to apply.</summary>
    public NameConvention Convention { get; } = convention;
}
