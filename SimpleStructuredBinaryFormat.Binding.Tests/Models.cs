using System.Collections.Generic;
using SimpleStructuredBinaryFormat.Binding;

[SsbfSerializable]
public partial class PersonModel
{
    public string Name { get; set; } = "";
    public int Age { get; set; }
    public bool Active { get; set; }
    public double Score { get; set; }
    public List<string>? Tags { get; set; }
    public byte[]? Avatar { get; set; }
    public AddressModel? Address { get; set; }

    [SsbfProperty("extra_notes")]
    public string? Notes { get; set; }

    [SsbfIgnore]
    public string InternalData { get; set; } = "secret";
}

[SsbfSerializable]
public partial class AddressModel
{
    public string City { get; set; } = "";
    public string? Zip { get; set; }
}

[SsbfSerializable]
public partial class NumericModel
{
    public sbyte  I8  { get; set; }
    public short  I16 { get; set; }
    public int    I32 { get; set; }
    public long   I64 { get; set; }
    public byte   U8  { get; set; }
    public ushort U16 { get; set; }
    public uint   U32 { get; set; }
    public ulong  U64 { get; set; }
    public Half   F16 { get; set; }
    public float  F32 { get; set; }
    public double F64 { get; set; }
}

[SsbfSerializable]
[SsbfNameConvention(NameConvention.CamelCase)]
public partial class CamelCaseModel
{
    public string FirstName { get; set; } = "";
    public int MaxRetryCount { get; set; }

    [SsbfProperty("html_content")]  // explicit override wins over convention
    public string HTMLContent { get; set; } = "";
}

[SsbfSerializable]
[SsbfNameConvention(NameConvention.SnakeCase)]
public partial class SnakeCaseModel
{
    public string FirstName { get; set; } = "";
    public int MaxRetryCount { get; set; }
    public string HTMLParser { get; set; } = "";
}

[SsbfSerializable]
[SsbfNameConvention(NameConvention.ScreamingSnakeCase)]
public partial class ScreamingSnakeCaseModel
{
    public string FirstName { get; set; } = "";
    public int MaxRetryCount { get; set; }
}

[SsbfSerializable]
[SsbfNameConvention(NameConvention.KebabCase)]
public partial class KebabCaseModel
{
    public string FirstName { get; set; } = "";
    public int MaxRetryCount { get; set; }
}

[SsbfSerializable]
[SsbfNameConvention(NameConvention.PascalCase)]
public partial class PascalCaseModel
{
    public string firstName { get; set; } = "";
    public int maxRetryCount { get; set; }
}

[SsbfSerializable]
public partial class NullableModel
{
    public int? OptInt { get; set; }
    public bool? OptBool { get; set; }
    public double? OptDouble { get; set; }
}
