using System.Collections;

namespace SimpleStructuredBinaryFormat;

public class SsbfArray : SsbfNode, IList<SsbfNode?>
{
    public override NodeType Type => NodeType.Array;
    
    public int Count => nodes.Count;
    public bool IsReadOnly => false;

    private readonly List<SsbfNode?> nodes;
    
    public SsbfArray()
    {
        nodes = new List<SsbfNode?>();
    }
    
    public SsbfArray(int capacity)
    {
        nodes = new List<SsbfNode?>(capacity);
    }
    
    public SsbfArray(IEnumerable<SsbfNode?> collection)
    {
        nodes = new List<SsbfNode?>(collection);
    }
    
    public IEnumerator<SsbfNode?> GetEnumerator()
    {
        return nodes.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public void Add(SsbfNode? item)
    {
        nodes.Add(item);
    }

    public void Clear()
    {
        nodes.Clear();
    }

    public bool Contains(SsbfNode? item)
    {
        return nodes.Contains(item);
    }

    public void CopyTo(SsbfNode?[] array, int arrayIndex)
    {
        nodes.CopyTo(array, arrayIndex);
    }

    public bool Remove(SsbfNode? item)
    {
        return nodes.Remove(item);
    }
    
    public int IndexOf(SsbfNode? item)
    {
        return nodes.IndexOf(item);
    }

    public void Insert(int index, SsbfNode? item)
    {
        nodes.Insert(index, item);
    }

    public void RemoveAt(int index)
    {
        nodes.RemoveAt(index);
    }

    public new SsbfNode? this[int index]
    {
        get => nodes[index];
        set
        {
            // Fill the gap with nulls
            while (nodes.Count <= index)
                nodes.Add(null);
            nodes[index] = value;
        }
    }

    public override string ToString()
    {
        return $"[ {string.Join(", ", nodes.Select(x => x?.ToString() ?? "null"))} ]";
    }
}