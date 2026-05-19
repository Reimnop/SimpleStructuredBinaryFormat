namespace SimpleStructuredBinaryFormat.Binding;

/// <summary>
/// Controls how property names are transformed when serializing to and deserializing from SSBF.
/// Applied via <see cref="SsbfNameConventionAttribute"/>.
/// Individual <see cref="SsbfPropertyAttribute"/> overrides always take priority.
/// </summary>
public enum NameConvention
{
    /// <summary>Names are written as-is (no transformation).</summary>
    None,

    /// <summary>Names are transformed to <c>PascalCase</c>. e.g. <c>myProperty</c> → <c>MyProperty</c></summary>
    PascalCase,

    /// <summary>Names are transformed to <c>camelCase</c>. e.g. <c>MyProperty</c> → <c>myProperty</c></summary>
    CamelCase,

    /// <summary>Names are transformed to <c>snake_case</c>. e.g. <c>MyProperty</c> → <c>my_property</c></summary>
    SnakeCase,

    /// <summary>Names are transformed to <c>SCREAMING_SNAKE_CASE</c>. e.g. <c>MyProperty</c> → <c>MY_PROPERTY</c></summary>
    ScreamingSnakeCase,

    /// <summary>Names are transformed to <c>kebab-case</c>. e.g. <c>MyProperty</c> → <c>my-property</c></summary>
    KebabCase,
}
