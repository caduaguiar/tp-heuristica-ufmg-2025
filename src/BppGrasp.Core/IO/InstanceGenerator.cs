using BppGrasp.Core.Models;

namespace BppGrasp.Core.IO;

public sealed class InstanceGenerator
{
    private readonly Random _random;

    public InstanceGenerator(int seed = 42)
    {
        _random = new Random(seed);
    }

    public BppInstance GenerateRandomInstance(
        string name,
        int itemCount,
        double binCapacity,
        double minWeight,
        double maxWeight,
        int precedenceCount)
    {
        var instance = new BppInstance(name, binCapacity);

        for (int i = 0; i < itemCount; i++)
        {
            var weight = minWeight + (maxWeight - minWeight) * _random.NextDouble();
            instance.AddItem(new Item(i, weight));
        }

        var addedPrecedences = new HashSet<(int, int)>();

        for (int i = 0; i < precedenceCount; i++)
        {
            int itemA, itemB;
            int attempts = 0;

            do
            {
                itemA = _random.Next(itemCount);
                itemB = _random.Next(itemCount);
                attempts++;

                if (attempts > 1000)
                    break;

            } while (itemA == itemB || addedPrecedences.Contains((itemA, itemB)));

            if (itemA != itemB && !addedPrecedences.Contains((itemA, itemB)))
            {
                var minSeparation = _random.Next(0, 3);
                instance.AddPrecedence(new PrecedenceConstraint(itemA, itemB, minSeparation));
                addedPrecedences.Add((itemA, itemB));
            }
        }

        return instance;
    }

    public BppInstance GenerateSimpleInstance()
    {
        var instance = new BppInstance("Simple", 100.0);

        instance.AddItem(new Item(0, 50));
        instance.AddItem(new Item(1, 40));
        instance.AddItem(new Item(2, 35));
        instance.AddItem(new Item(3, 30));
        instance.AddItem(new Item(4, 25));
        instance.AddItem(new Item(5, 20));

        instance.AddPrecedence(new PrecedenceConstraint(0, 1, 1));
        instance.AddPrecedence(new PrecedenceConstraint(2, 3, 0));

        return instance;
    }
}
