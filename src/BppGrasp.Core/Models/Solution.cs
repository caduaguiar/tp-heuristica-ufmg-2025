namespace BppGrasp.Core.Models;

public sealed class Solution
{
    private readonly List<Bin> _bins;
    private readonly Dictionary<int, int> _itemToBinMap;

    public IReadOnlyList<Bin> Bins => _bins.AsReadOnly();
    public int BinCount => _bins.Count;
    public BppInstance Instance { get; }
    public bool IsFeasible { get; private set; }

    public Solution(BppInstance instance)
    {
        Instance = instance ?? throw new ArgumentNullException(nameof(instance));
        _bins = new List<Bin>();
        _itemToBinMap = new Dictionary<int, int>();
        IsFeasible = false;
    }

    public void AddBin(Bin bin)
    {
        _bins.Add(bin);

        foreach (var item in bin.Items)
        {
            _itemToBinMap[item.Id] = bin.Id;
        }
    }

    public int GetBinForItem(int itemId)
    {
        return _itemToBinMap.TryGetValue(itemId, out var binId) ? binId : -1;
    }

    public void ValidateSolution()
    {
        IsFeasible = CheckCapacityConstraints() && CheckPrecedenceConstraints();
    }

    private bool CheckCapacityConstraints()
    {
        return _bins.All(bin => bin.UsedCapacity <= bin.Capacity);
    }

    private bool CheckPrecedenceConstraints()
    {
        foreach (var precedence in Instance.Precedences)
        {
            var binA = GetBinForItem(precedence.ItemA);
            var binB = GetBinForItem(precedence.ItemB);

            if (binA == -1 || binB == -1)
                return false;

            if (!precedence.IsSatisfied(binA, binB))
                return false;
        }

        return true;
    }

    public void Clear()
    {
        _bins.Clear();
        _itemToBinMap.Clear();
        IsFeasible = false;
    }

    public Solution Clone()
    {
        var clone = new Solution(Instance);

        foreach (var bin in _bins)
        {
            clone.AddBin(bin.Clone());
        }

        clone.IsFeasible = IsFeasible;
        return clone;
    }

    public override string ToString() =>
        $"Solution: {BinCount} bins, Feasible={IsFeasible}";
}
