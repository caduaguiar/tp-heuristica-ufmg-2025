namespace BppGrasp.Core.Models;

public sealed class Bin
{
    private readonly List<Item> _items;

    public int Id { get; }
    public double Capacity { get; }
    public IReadOnlyList<Item> Items => _items.AsReadOnly();
    public double RemainingCapacity => Capacity - UsedCapacity;
    public double UsedCapacity => _items.Sum(i => i.Weight);
    public bool IsEmpty => _items.Count == 0;

    public Bin(int id, double capacity)
    {
        if (id < 0)
            throw new ArgumentException("Bin ID must be non-negative", nameof(id));

        if (capacity <= 0)
            throw new ArgumentException("Bin capacity must be positive", nameof(capacity));

        Id = id;
        Capacity = capacity;
        _items = new List<Item>();
    }

    public bool CanFit(Item item)
    {
        return RemainingCapacity >= item.Weight;
    }

    public bool CanFit(double weight)
    {
        return RemainingCapacity >= weight;
    }

    public bool AddItem(Item item)
    {
        if (!CanFit(item))
            return false;

        _items.Add(item);
        return true;
    }

    public void RemoveItem(Item item)
    {
        _items.Remove(item);
    }

    public void Clear()
    {
        _items.Clear();
    }

    public Bin Clone()
    {
        var clonedBin = new Bin(Id, Capacity);
        foreach (var item in _items)
        {
            clonedBin.AddItem(item);
        }
        return clonedBin;
    }

    public override string ToString() =>
        $"Bin {Id}: {_items.Count} items, {UsedCapacity:F2}/{Capacity:F2} used";
}
