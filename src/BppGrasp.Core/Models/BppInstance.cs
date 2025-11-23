namespace BppGrasp.Core.Models;

public sealed class BppInstance
{
    public string Name { get; }
    public List<Item> Items { get; }
    public double BinCapacity { get; }
    public List<PrecedenceConstraint> Precedences { get; }

    public BppInstance(string name, double binCapacity)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Instance name cannot be empty", nameof(name));

        if (binCapacity <= 0)
            throw new ArgumentException("Bin capacity must be positive", nameof(binCapacity));

        Name = name;
        BinCapacity = binCapacity;
        Items = new List<Item>();
        Precedences = new List<PrecedenceConstraint>();
    }

    public void AddItem(Item item)
    {
        Items.Add(item);
    }

    public void AddPrecedence(PrecedenceConstraint precedence)
    {
        Precedences.Add(precedence);
    }

    public BppInstance Clone()
    {
        var clone = new BppInstance(Name, BinCapacity);
        clone.Items.AddRange(Items);
        clone.Precedences.AddRange(Precedences);
        return clone;
    }

    public override string ToString() =>
        $"{Name}: {Items.Count} items, {Precedences.Count} precedences, capacity={BinCapacity}";
}
