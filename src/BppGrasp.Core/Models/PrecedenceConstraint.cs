namespace BppGrasp.Core.Models;

public sealed class PrecedenceConstraint
{
    public int ItemA { get; }
    public int ItemB { get; }
    public int MinSeparation { get; }

    public PrecedenceConstraint(int itemA, int itemB, int minSeparation)
    {
        if (itemA < 0 || itemB < 0)
            throw new ArgumentException("Item IDs must be non-negative");

        if (itemA == itemB)
            throw new ArgumentException("Items A and B cannot be the same");

        ItemA = itemA;
        ItemB = itemB;
        MinSeparation = minSeparation;
    }

    public bool IsSatisfied(int binA, int binB)
    {
        return binA - binB >= MinSeparation;
    }

    public override string ToString() => $"({ItemA}, {ItemB}, {MinSeparation})";
}
