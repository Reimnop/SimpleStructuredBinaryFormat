using SimpleStructuredBinaryFormat;

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

using (var stream = File.Open("employees.ssbf", FileMode.Open))
{
    var obj2 = SsbfRead.ReadFromStream(stream);
    Console.WriteLine(obj2);
}
