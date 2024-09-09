namespace SimpleStructuredBinaryFormat;

public static class Extension
{
    public static T Get<T>(this SsbfNode node)
    {
        if (node is T value)
            return value;
        if (node is SsbfValue<T> ssbfValue)
            return ssbfValue.Value;
        if (typeof(T) == typeof(byte[]) && node is SsbfByteArray ssbfByteArray)
            return (T)(object)ssbfByteArray.Data;
        throw new InvalidCastException();
    }
    
    public static T GetOrDefault<T>(this SsbfNode node, T defaultValue)
    {
        if (node is T value)
            return value;
        if (node is SsbfValue<T> ssbfValue)
            return ssbfValue.Value;
        if (typeof(T) == typeof(byte[]) && node is SsbfByteArray ssbfByteArray)
            return (T)(object)ssbfByteArray.Data;
        return defaultValue;
    }
}