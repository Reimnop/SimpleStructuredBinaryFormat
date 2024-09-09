# Simple Structured Binary Format

Simple Structured Binary Format is a simple, efficient, structured binary data format. It mimics JSON, with additional types and compression support.

This library provides a C# interface for it, though feel free to port it to any other languages or write your own implementation!

## Features

- Supports a bunch of data types: `object`, `array`, `bool`, `sbyte`, `short`, `int`, `long`, `byte`, `ushort`, `uint`, `ulong`, `Half`, `float`, `double`, `string` and `byte[]`
- Supports compression methods: None, Gzip, and Deflate.
- Simple API for reading and writing SSBF data.

## Installation

Get it on [NuGet](https://www.nuget.org/packages/SimpleStructuredBinaryFormat/)

To use SSBF in your project, add a reference to the `SimpleStructuredBinaryFormat` namespace.

## Usage

### Writing Data

```csharp
using SimpleStructuredBinaryFormat;
using System.IO;
using System.Linq;

var obj = new SsbfObject
{
    ["employees"] = new SsbfArray
    {
        new SsbfObject
        {
            ["name"] = "John Doe",
            ["age"] = 30,
            ["gender"] = "male",
            ["married"] = false,
            ["salary"] = 50000.0
        },
        new SsbfObject
        {
            ["name"] = "Mary Jane",
            ["age"] = 28,
            ["gender"] = "female",
            ["married"] = true,
            ["salary"] = 60000.0
        },
    },
    ["data"] = Enumerable.Range(0, 10000).Select(x => (byte)(x % 256)).ToArray()
};

using (var stream = File.Open("employees.ssbf", FileMode.Create))
{
    SsbfWrite.WriteToStream(stream, obj, Compression.Gzip);
}
```

### Reading Data

```csharp
using SimpleStructuredBinaryFormat;
using System.IO;

using (var stream = File.Open("employees.ssbf", FileMode.Open))
{
    var obj = SsbfRead.ReadFromStream(stream);
    Console.WriteLine(obj);
}
```

## API Reference

### Classes

- `SsbfObject`: Represents a structured object with key-value pairs.
- `SsbfArray`: Represents an array of SSBF nodes.
- `SsbfByteArray`: Represents a byte array.
- `SsbfWrite`: Provides methods to write SSBF data to a stream.
- `SsbfRead`: Provides methods to read SSBF data from a stream.

### Methods

- `SsbfWrite.WriteToStream(Stream stream, SsbfObject obj, Compression compression)`: Writes the specified SSBF node to the stream.
- `SsbfRead.ReadFromStream(Stream stream)`: Reads an SSBF node from the stream.

### Enums

- `Compression`: Specifies the compression method. Values: `None`, `Gzip` (recommended), `Deflate`.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.