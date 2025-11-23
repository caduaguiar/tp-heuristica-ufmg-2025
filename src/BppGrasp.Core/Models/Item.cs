namespace BppGrasp.Core.Models;

public sealed class Item
{
    public int Id { get; }
    public double Weight { get; }

    public Item(int id, double weight)
    {
        if (id < 0)
            throw new ArgumentException("Item ID must be non-negative", nameof(id));

        if (weight <= 0)
            throw new ArgumentException("Item weight must be positive", nameof(weight));

        Id = id;
        Weight = weight;
    }

    public override string ToString() => $"Item {Id} (w={Weight:F2})";

    public override bool Equals(object? obj) => obj is Item item && Id == item.Id;

    public override int GetHashCode() => Id.GetHashCode();
}
