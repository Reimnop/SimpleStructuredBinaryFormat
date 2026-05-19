using Xunit;

namespace SimpleStructuredBinaryFormat.Binding.Tests;

public class BindingTests
{
    private static T RoundTrip<T>(T value, ISsbfBinder<T> binder, bool compress = false)
    {
        var ms = new MemoryStream();
        SsbfSerializer.Serialize(ms, value, binder, useCompression: compress, leaveOpen: true);
        ms.Position = 0;
        return SsbfSerializer.Deserialize(ms, binder, leaveOpen: false);
    }

    private static IEnumerable<string> WireKeys(MemoryStream ms)
    {
        ms.Position = 0;
        using var reader = new SsbfReader(ms, leaveOpen: true);
        var keys = new List<string>();
        while (reader.Read())
            if (reader.TokenType == SsbfTokenType.PropertyName)
                keys.Add(reader.GetPropertyName());
        return keys;
    }

    [Fact]
    public void FlatObjectRoundTrips()
    {
        var r = RoundTrip(new PersonModel { Name = "Alice", Age = 30, Active = true, Score = 9.5 }, PersonModel.Binder);
        Assert.Equal("Alice", r.Name);
        Assert.Equal(30, r.Age);
        Assert.True(r.Active);
        Assert.Equal(9.5, r.Score);
    }

    [Fact]
    public void NestedObjectRoundTrips()
    {
        var r = RoundTrip(new PersonModel
        {
            Name = "Bob", Age = 25,
            Address = new AddressModel { City = "Springfield", Zip = "12345" }
        }, PersonModel.Binder);
        Assert.NotNull(r.Address);
        Assert.Equal("Springfield", r.Address!.City);
        Assert.Equal("12345", r.Address.Zip);
    }

    [Fact]
    public void ListStringRoundTrips()
    {
        var r = RoundTrip(new PersonModel { Tags = new List<string> { "admin", "user", "moderator" } }, PersonModel.Binder);
        Assert.NotNull(r.Tags);
        Assert.Equal(3, r.Tags!.Count);
        Assert.Equal("admin", r.Tags[0]);
        Assert.Equal("user", r.Tags[1]);
        Assert.Equal("moderator", r.Tags[2]);
    }

    [Fact]
    public void ByteArrayRoundTrips()
    {
        var r = RoundTrip(new PersonModel { Avatar = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 } }, PersonModel.Binder);
        Assert.NotNull(r.Avatar);
        Assert.Equal(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, r.Avatar!);
    }

    [Fact]
    public void SsbfPropertyKeyOverrideWrittenToWire()
    {
        var ms = new MemoryStream();
        SsbfSerializer.Serialize(ms, new PersonModel { Notes = "hello" }, PersonModel.Binder, leaveOpen: true);
        ms.Position = 0;
        using var reader = new SsbfReader(ms);
        bool found = false;
        while (reader.Read())
            if (reader.TokenType == SsbfTokenType.PropertyName && reader.GetPropertyName() == "extra_notes")
            { found = true; break; }
        Assert.True(found, "expected key 'extra_notes' on wire");
    }

    [Fact]
    public void SsbfPropertyKeyOverrideRoundTripsCorrectly()
    {
        var r = RoundTrip(new PersonModel { Notes = "overridden key" }, PersonModel.Binder);
        Assert.Equal("overridden key", r.Notes);
    }

    [Fact]
    public void SsbfIgnoreMemberExcludedFromWire()
    {
        var ms = new MemoryStream();
        SsbfSerializer.Serialize(ms, new PersonModel { InternalData = "secret" }, PersonModel.Binder, leaveOpen: true);
        ms.Position = 0;
        using var reader = new SsbfReader(ms);
        bool found = false;
        while (reader.Read())
            if (reader.TokenType == SsbfTokenType.PropertyName && reader.GetPropertyName() == "InternalData")
            { found = true; break; }
        Assert.False(found, "InternalData should not appear on wire");
    }

    [Fact]
    public void UnknownPropertiesSkippedOnDeserialize()
    {
        var ms = new MemoryStream();
        using (var w = new SsbfWriter(ms, leaveOpen: true))
        {
            w.WriteStartObject();
            w.WritePropertyName("Name");          w.WriteString("Carol");
            w.WritePropertyName("Age");           w.WriteInteger(40);
            w.WritePropertyName("unknown_field"); w.WriteString("ignored");
            w.WritePropertyName("Active");        w.WriteBoolean(false);
            w.WriteEndObject();
            w.Flush();
        }
        ms.Position = 0;
        var r = SsbfSerializer.Deserialize(ms, PersonModel.Binder);
        Assert.Equal("Carol", r.Name);
        Assert.Equal(40, r.Age);
    }

    [Fact]
    public void UnknownNestedObjectPropertySkipped()
    {
        var ms = new MemoryStream();
        using (var w = new SsbfWriter(ms, leaveOpen: true))
        {
            w.WriteStartObject();
            w.WritePropertyName("Name"); w.WriteString("Dave");
            w.WritePropertyName("mystery");
            w.WriteStartObject();
            w.WritePropertyName("deep"); w.WriteInteger(99);
            w.WriteEndObject();
            w.WriteEndObject();
            w.Flush();
        }
        ms.Position = 0;
        var r = SsbfSerializer.Deserialize(ms, PersonModel.Binder);
        Assert.Equal("Dave", r.Name);
    }

    [Fact]
    public void AllNumericTypesRoundTrip()
    {
        var orig = new NumericModel
        {
            I8 = sbyte.MinValue, I16 = short.MaxValue, I32 = -1_000_000, I64 = long.MinValue,
            U8 = byte.MaxValue,  U16 = ushort.MaxValue, U32 = uint.MaxValue, U64 = ulong.MaxValue,
            F16 = (Half)(-1.0),  F32 = float.Epsilon, F64 = double.MaxValue,
        };
        var r = RoundTrip(orig, NumericModel.Binder);
        Assert.Equal(sbyte.MinValue, r.I8);
        Assert.Equal(short.MaxValue, r.I16);
        Assert.Equal(-1_000_000, r.I32);
        Assert.Equal(long.MinValue, r.I64);
        Assert.Equal(byte.MaxValue, r.U8);
        Assert.Equal(ushort.MaxValue, r.U16);
        Assert.Equal(uint.MaxValue, r.U32);
        Assert.Equal(ulong.MaxValue, r.U64);
        Assert.Equal((Half)(-1.0), r.F16);
        Assert.Equal(float.Epsilon, r.F32);
        Assert.Equal(double.MaxValue, r.F64);
    }

    [Fact]
    public void NullableWithValueRoundTrips()
    {
        var r = RoundTrip(new NullableModel { OptInt = 42, OptBool = true, OptDouble = 3.14 }, NullableModel.Binder);
        Assert.Equal(42, r.OptInt);
        Assert.True(r.OptBool);
        Assert.Equal(3.14, r.OptDouble);
    }

    [Fact]
    public void NullableNullValuesRoundTrip()
    {
        var r = RoundTrip(new NullableModel { OptInt = null, OptBool = null, OptDouble = null }, NullableModel.Binder);
        Assert.Null(r.OptInt);
        Assert.Null(r.OptBool);
        Assert.Null(r.OptDouble);
    }

    [Fact]
    public void CompressionRoundTrips()
    {
        var r = RoundTrip(new PersonModel { Name = "Dave", Age = 99, Active = false, Score = 1.23 }, PersonModel.Binder, compress: true);
        Assert.Equal("Dave", r.Name);
        Assert.Equal(99, r.Age);
        Assert.False(r.Active);
        Assert.Equal(1.23, r.Score);
    }

    [Fact]
    public void FullPersonModelRoundTrip()
    {
        var orig = new PersonModel
        {
            Name = "Eve", Age = 28, Active = true, Score = 7.7,
            Notes = "annotated",
            Tags = new List<string> { "alpha", "beta" },
            Avatar = new byte[] { 1, 2, 3 },
            Address = new AddressModel { City = "Metropolis", Zip = null },
            InternalData = "should be ignored"
        };
        var r = RoundTrip(orig, PersonModel.Binder);
        Assert.Equal("Eve", r.Name);
        Assert.Equal(28, r.Age);
        Assert.True(r.Active);
        Assert.Equal(7.7, r.Score);
        Assert.Equal("annotated", r.Notes);
        Assert.Equal(2, r.Tags!.Count);
        Assert.Equal("alpha", r.Tags[0]);
        Assert.Equal(new byte[] { 1, 2, 3 }, r.Avatar!);
        Assert.Equal("Metropolis", r.Address!.City);
        Assert.Null(r.Address.Zip);
        Assert.Equal("secret", r.InternalData); // property initializer default — not from wire
    }

    [Fact]
    public void AddressModelStandaloneRoundTrips()
    {
        var r = RoundTrip(new AddressModel { City = "Gotham", Zip = "00001" }, AddressModel.Binder);
        Assert.Equal("Gotham", r.City);
        Assert.Equal("00001", r.Zip);
    }

    [Fact]
    public void BinderInstanceIsSingleton()
    {
        Assert.True(ReferenceEquals(PersonModel.Binder, PersonModel.Binder));
    }

    [Fact]
    public void CamelCaseConventionTransformsKeys()
    {
        var ms = new MemoryStream();
        SsbfSerializer.Serialize(ms, new CamelCaseModel { FirstName = "x", MaxRetryCount = 1, HTMLContent = "y" },
            CamelCaseModel.Binder, leaveOpen: true);
        var keys = WireKeys(ms).ToList();
        Assert.Contains("firstName", keys);
        Assert.Contains("maxRetryCount", keys);
        Assert.Contains("html_content", keys);
        Assert.DoesNotContain("FirstName", keys);
    }

    [Fact]
    public void CamelCaseRoundTrips()
    {
        var r = RoundTrip(new CamelCaseModel { FirstName = "Alice", MaxRetryCount = 3, HTMLContent = "<b>" },
            CamelCaseModel.Binder);
        Assert.Equal("Alice", r.FirstName);
        Assert.Equal(3, r.MaxRetryCount);
        Assert.Equal("<b>", r.HTMLContent);
    }

    [Fact]
    public void SnakeCaseConventionTransformsKeys()
    {
        var ms = new MemoryStream();
        SsbfSerializer.Serialize(ms, new SnakeCaseModel { FirstName = "x", MaxRetryCount = 1, HTMLParser = "y" },
            SnakeCaseModel.Binder, leaveOpen: true);
        var keys = WireKeys(ms).ToList();
        Assert.Equal("first_name", keys.First(k => k.Contains("first")));
        Assert.Equal("max_retry_count", keys.First(k => k.Contains("max")));
        Assert.Equal("html_parser", keys.First(k => k.Contains("html")));
    }

    [Fact]
    public void SnakeCaseRoundTrips()
    {
        var r = RoundTrip(new SnakeCaseModel { FirstName = "Bob", MaxRetryCount = 5, HTMLParser = "lxml" },
            SnakeCaseModel.Binder);
        Assert.Equal("Bob", r.FirstName);
        Assert.Equal(5, r.MaxRetryCount);
        Assert.Equal("lxml", r.HTMLParser);
    }

    [Fact]
    public void ScreamingSnakeCaseConventionTransformsKeys()
    {
        var ms = new MemoryStream();
        SsbfSerializer.Serialize(ms, new ScreamingSnakeCaseModel { FirstName = "x", MaxRetryCount = 1 },
            ScreamingSnakeCaseModel.Binder, leaveOpen: true);
        var keys = WireKeys(ms).ToList();
        Assert.Contains("FIRST_NAME", keys);
        Assert.Contains("MAX_RETRY_COUNT", keys);
    }

    [Fact]
    public void ScreamingSnakeCaseRoundTrips()
    {
        var r = RoundTrip(new ScreamingSnakeCaseModel { FirstName = "Carol", MaxRetryCount = 7 },
            ScreamingSnakeCaseModel.Binder);
        Assert.Equal("Carol", r.FirstName);
        Assert.Equal(7, r.MaxRetryCount);
    }

    [Fact]
    public void KebabCaseConventionTransformsKeys()
    {
        var ms = new MemoryStream();
        SsbfSerializer.Serialize(ms, new KebabCaseModel { FirstName = "x", MaxRetryCount = 1 },
            KebabCaseModel.Binder, leaveOpen: true);
        var keys = WireKeys(ms).ToList();
        Assert.Contains("first-name", keys);
        Assert.Contains("max-retry-count", keys);
    }

    [Fact]
    public void KebabCaseRoundTrips()
    {
        var r = RoundTrip(new KebabCaseModel { FirstName = "Dave", MaxRetryCount = 2 },
            KebabCaseModel.Binder);
        Assert.Equal("Dave", r.FirstName);
        Assert.Equal(2, r.MaxRetryCount);
    }

    [Fact]
    public void PascalCaseConventionTransformsCamelCaseMemberNames()
    {
        var ms = new MemoryStream();
        SsbfSerializer.Serialize(ms, new PascalCaseModel { firstName = "x", maxRetryCount = 1 },
            PascalCaseModel.Binder, leaveOpen: true);
        var keys = WireKeys(ms).ToList();
        Assert.Contains("FirstName", keys);
        Assert.Contains("MaxRetryCount", keys);
    }

    [Fact]
    public void PascalCaseRoundTrips()
    {
        var r = RoundTrip(new PascalCaseModel { firstName = "Eve", maxRetryCount = 9 },
            PascalCaseModel.Binder);
        Assert.Equal("Eve", r.firstName);
        Assert.Equal(9, r.maxRetryCount);
    }
}
