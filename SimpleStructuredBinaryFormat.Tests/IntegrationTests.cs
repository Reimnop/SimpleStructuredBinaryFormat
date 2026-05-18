namespace SimpleStructuredBinaryFormat.Tests;

/// <summary>
/// End-to-end tests that write realistic structured documents and verify
/// the full token stream produced by the reader.
/// </summary>
public class IntegrationTests
{
    // -----------------------------------------------------------------------
    // "Person" document
    // -----------------------------------------------------------------------

    private static void WritePersonDocument(SsbfWriter w)
    {
        // {
        //   "name": "Alice",
        //   "age": 30,
        //   "active": true,
        //   "score": 98.6,
        //   "tags": ["admin", "user"],
        //   "avatar": <bytes>,
        //   "address": {
        //     "city": "Wonderland",
        //     "zip": null
        //   }
        // }
        w.WriteStartObject();

        w.WritePropertyName("name");   w.WriteString("Alice");
        w.WritePropertyName("age");    w.WriteInteger(30);
        w.WritePropertyName("active"); w.WriteBoolean(true);
        w.WritePropertyName("score");  w.WriteDouble(98.6);

        w.WritePropertyName("tags");
        w.WriteStartArray();
        w.WriteString("admin");
        w.WriteString("user");
        w.WriteEndArray();

        w.WritePropertyName("avatar");
        w.WriteByteArray([0xFF, 0xD8, 0xFF, 0xE0]);

        w.WritePropertyName("address");
        w.WriteStartObject();
        w.WritePropertyName("city"); w.WriteString("Wonderland");
        w.WritePropertyName("zip");  w.WriteNull();
        w.WriteEndObject();

        w.WriteEndObject();
    }

    [Fact]
    public void PersonDocument_RoundTrips_Uncompressed()
    {
        using var reader = TestHelpers.WriteAndRead(WritePersonDocument, useCompression: false);
        VerifyPersonDocument(reader);
    }

    [Fact]
    public void PersonDocument_RoundTrips_Compressed()
    {
        using var reader = TestHelpers.WriteAndRead(WritePersonDocument, useCompression: true);
        VerifyPersonDocument(reader);
    }

    private static void VerifyPersonDocument(SsbfReader reader)
    {
        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("name",   reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.String);       Assert.Equal("Alice",  reader.GetString());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("age",    reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);      Assert.Equal(30,       reader.GetInteger());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("active", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Boolean);      Assert.Equal(true,     reader.GetBoolean());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("score",  reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Double);       Assert.Equal(98.6,     reader.GetDouble());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("tags",   reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.String);       Assert.Equal("admin",  reader.GetString());
        TestHelpers.AssertRead(reader, SsbfTokenType.String);       Assert.Equal("user",   reader.GetString());
        TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("avatar", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.ByteArray);
        Assert.Equal(new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }, reader.GetByteArray());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("address",    reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("city",        reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.String);       Assert.Equal("Wonderland",  reader.GetString());
        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("zip",         reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Null);
        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);

        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertEnd(reader);
    }

    // -----------------------------------------------------------------------
    // All numeric types in one document
    // -----------------------------------------------------------------------

    [Fact]
    public void AllNumericTypes_RoundTrip()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("i8");   w.WriteSByte(sbyte.MinValue);
            w.WritePropertyName("i16");  w.WriteShort(short.MaxValue);
            w.WritePropertyName("i32");  w.WriteInteger(-1_000_000);
            w.WritePropertyName("i64");  w.WriteLong(long.MinValue);
            w.WritePropertyName("u8");   w.WriteByte(byte.MaxValue);
            w.WritePropertyName("u16");  w.WriteUShort(ushort.MaxValue);
            w.WritePropertyName("u32");  w.WriteUInteger(uint.MaxValue);
            w.WritePropertyName("u64");  w.WriteULong(ulong.MaxValue);
            w.WritePropertyName("f16");  w.WriteHalf((Half)(-1.0));
            w.WritePropertyName("f32");  w.WriteSingle(float.Epsilon);
            w.WritePropertyName("f64");  w.WriteDouble(double.MaxValue);
            w.WriteEndObject();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("i8",  reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.SByte);        Assert.Equal(sbyte.MinValue, reader.GetSByte());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("i16", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Short);        Assert.Equal(short.MaxValue, reader.GetShort());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("i32", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);      Assert.Equal(-1_000_000, reader.GetInteger());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("i64", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Long);         Assert.Equal(long.MinValue, reader.GetLong());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("u8",  reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Byte);         Assert.Equal(byte.MaxValue, reader.GetByte());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("u16", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.UShort);       Assert.Equal(ushort.MaxValue, reader.GetUShort());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("u32", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.UInteger);     Assert.Equal(uint.MaxValue, reader.GetUInteger());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("u64", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.ULong);        Assert.Equal(ulong.MaxValue, reader.GetULong());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("f16", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.HalfFloat);    Assert.Equal((Half)(-1.0), reader.GetHalf());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("f32", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Single);       Assert.Equal(float.Epsilon, reader.GetSingle());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("f64", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Double);       Assert.Equal(double.MaxValue, reader.GetDouble());

        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertEnd(reader);
    }

    // -----------------------------------------------------------------------
    // LeaveOpen = true: stream must remain open after reader disposal
    // -----------------------------------------------------------------------

    [Fact]
    public void LeaveOpen_True_StreamRemainsOpen()
    {
        var ms = new MemoryStream();
        using (var writer = new SsbfWriter(ms, leaveOpen: true))
        {
            writer.WriteInteger(7);
            writer.Flush();
        }

        ms.Position = 0;
        using (var reader = new SsbfReader(ms, leaveOpen: true))
        {
            reader.Read();
            Assert.Equal(7, reader.GetInteger());
        }

        // Stream should still be readable/open after reader disposal.
        Assert.True(ms.CanRead, "Stream should still be open");
    }
}
