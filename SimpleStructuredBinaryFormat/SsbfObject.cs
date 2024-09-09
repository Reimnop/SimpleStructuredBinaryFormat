using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace SimpleStructuredBinaryFormat;

/// <summary>
/// Represents an object, which is a collection of key-value pairs of string key names and <see cref="SsbfNode"/> values.
/// </summary>
public class SsbfObject : SsbfNode, IDictionary<string, SsbfNode>
{
    public override NodeType Type => NodeType.Object;
    
    public int Count => nodes.Count;
    public bool IsReadOnly => false;
    public ICollection<string> Keys => nodes.Keys;
    public ICollection<SsbfNode> Values => nodes.Values;

    private readonly Dictionary<string, SsbfNode> nodes;
    
    public SsbfObject()
    {
        nodes = new Dictionary<string, SsbfNode>();
    }
    
    public SsbfObject(int capacity)
    {
        nodes = new Dictionary<string, SsbfNode>(capacity);
    }
    
    public IEnumerator<KeyValuePair<string, SsbfNode>> GetEnumerator()
    {
        return nodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(KeyValuePair<string, SsbfNode> item)
    {
        ((ICollection<KeyValuePair<string, SsbfNode>>)nodes).Add(item);
    }

    public void Clear()
    {
        nodes.Clear();
    }

    public bool Contains(KeyValuePair<string, SsbfNode> item)
    {
        return ((ICollection<KeyValuePair<string, SsbfNode>>)nodes).Contains(item);
    }

    public void CopyTo(KeyValuePair<string, SsbfNode>[] array, int arrayIndex)
    {
        ((ICollection<KeyValuePair<string, SsbfNode>>)nodes).CopyTo(array, arrayIndex);
    }

    public bool Remove(KeyValuePair<string, SsbfNode> item)
    {
        return ((ICollection<KeyValuePair<string, SsbfNode>>)nodes).Remove(item);
    }
    
    public void Add(string key, SsbfNode value)
    {
        nodes.Add(key, value);
    }

    public bool ContainsKey(string key)
    {
        return nodes.ContainsKey(key);
    }

    public bool Remove(string key)
    {
        return nodes.Remove(key);
    }

    public bool TryGetValue(string key, [MaybeNullWhen(false)] out SsbfNode value)
    {
        return nodes.TryGetValue(key, out value);
    }

    public SsbfNode this[string key]
    {
        get => nodes[key];
        set => nodes[key] = value;
    }

    public override string ToString()
    {
        return $"{{ {string.Join(", ", nodes.Select(kv => $"{kv.Key}: {kv.Value}"))} }}";
    }
}