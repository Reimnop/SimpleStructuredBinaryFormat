namespace SimpleStructuredBinaryFormat.Tests;

public class StructureTests
{
    // -----------------------------------------------------------------------
    // Empty containers
    // -----------------------------------------------------------------------

    [Fact]
    public void EmptyObject_ProducesStartAndEnd()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WriteEndObject();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void EmptyArray_ProducesStartAndEnd()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartArray();
            w.WriteEndArray();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);
        TestHelpers.AssertEnd(reader);
    }

    // -----------------------------------------------------------------------
    // Flat object
    // -----------------------------------------------------------------------

    [Fact]
    public void FlatObject_TokenSequenceIsCorrect()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("x");
            w.WriteInteger(42);
            w.WritePropertyName("y");
            w.WriteBoolean(true);
            w.WriteEndObject();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName);
        Assert.Equal("x", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.Equal(42, reader.GetInteger());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName);
        Assert.Equal("y", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Boolean);
        Assert.Equal(true, reader.GetBoolean());

        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertEnd(reader);
    }

    // -----------------------------------------------------------------------
    // Flat array
    // -----------------------------------------------------------------------

    [Fact]
    public void FlatArray_TokenSequenceIsCorrect()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartArray();
            w.WriteInteger(1);
            w.WriteInteger(2);
            w.WriteInteger(3);
            w.WriteEndArray();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.Equal(1, reader.GetInteger());
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.Equal(2, reader.GetInteger());
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.Equal(3, reader.GetInteger());
        TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);
        TestHelpers.AssertEnd(reader);
    }

    // -----------------------------------------------------------------------
    // Nesting
    // -----------------------------------------------------------------------

    [Fact]
    public void NestedObject_InsideArray()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartArray();
            w.WriteStartObject();
            w.WritePropertyName("val");
            w.WriteString("hello");
            w.WriteEndObject();
            w.WriteEndArray();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName);
        Assert.Equal("val", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.String);
        Assert.Equal("hello", reader.GetString());
        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void NestedArray_InsideObject()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("items");
            w.WriteStartArray();
            w.WriteInteger(10);
            w.WriteInteger(20);
            w.WriteEndArray();
            w.WriteEndObject();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName);
        Assert.Equal("items", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.Equal(10, reader.GetInteger());
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.Equal(20, reader.GetInteger());
        TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void DeeplyNested_Object()
    {
        // object → object → object → scalar
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("a");
            w.WriteStartObject();
            w.WritePropertyName("b");
            w.WriteStartObject();
            w.WritePropertyName("c");
            w.WriteInteger(99);
            w.WriteEndObject();
            w.WriteEndObject();
            w.WriteEndObject();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName);
        Assert.Equal("a", reader.GetPropertyName());

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName);
        Assert.Equal("b", reader.GetPropertyName());

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName);
        Assert.Equal("c", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);
        Assert.Equal(99, reader.GetInteger());

        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void MultiplePropertiesOfDifferentTypes()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartObject();
            w.WritePropertyName("n");   w.WriteNull();
            w.WritePropertyName("b");   w.WriteBoolean(false);
            w.WritePropertyName("i");   w.WriteInteger(-1);
            w.WritePropertyName("s");   w.WriteString("str");
            w.WritePropertyName("arr"); w.WriteByteArray([0xDE, 0xAD]);
            w.WriteEndObject();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartObject);

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("n", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Null);

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("b", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Boolean);      Assert.Equal(false, reader.GetBoolean());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("i", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.Integer);      Assert.Equal(-1, reader.GetInteger());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("s", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.String);       Assert.Equal("str", reader.GetString());

        TestHelpers.AssertRead(reader, SsbfTokenType.PropertyName); Assert.Equal("arr", reader.GetPropertyName());
        TestHelpers.AssertRead(reader, SsbfTokenType.ByteArray);    Assert.Equal(new byte[] { 0xDE, 0xAD }, reader.GetByteArray());

        TestHelpers.AssertRead(reader, SsbfTokenType.EndObject);
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void ArrayOfMixedTypes()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartArray();
            w.WriteNull();
            w.WriteBoolean(true);
            w.WriteDouble(1.5);
            w.WriteString("hi");
            w.WriteEndArray();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.Null);
        TestHelpers.AssertRead(reader, SsbfTokenType.Boolean);  Assert.Equal(true, reader.GetBoolean());
        TestHelpers.AssertRead(reader, SsbfTokenType.Double);   Assert.Equal(1.5, reader.GetDouble());
        TestHelpers.AssertRead(reader, SsbfTokenType.String);   Assert.Equal("hi", reader.GetString());
        TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);
        TestHelpers.AssertEnd(reader);
    }

    [Fact]
    public void ArrayOfEmptyArrays()
    {
        using var reader = TestHelpers.WriteAndRead(w =>
        {
            w.WriteStartArray();
            w.WriteStartArray(); w.WriteEndArray();
            w.WriteStartArray(); w.WriteEndArray();
            w.WriteEndArray();
        });

        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray); TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.StartArray); TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);
        TestHelpers.AssertRead(reader, SsbfTokenType.EndArray);
        TestHelpers.AssertEnd(reader);
    }
}
