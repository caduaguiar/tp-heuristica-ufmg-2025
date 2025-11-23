using BppGrasp.Core.Models;
using BppGrasp.Core.Utils;

namespace BppGrasp.Core.Algorithms.ConstructiveHeuristics;

public sealed class BestFitDecreasingHeuristic : IConstructiveHeuristic
{
    private readonly ILogger _logger;

    public BestFitDecreasingHeuristic(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Solution Construct(BppInstance instance, double alpha, Random random)
    {
        var solution = new Solution(instance);
        var sortedItems = instance.Items.ToList();
        var bins = new List<Bin>();
        var itemToBin = new Dictionary<int, int>();

        foreach (var item in sortedItems)
        {
            var candidateBins = GetCandidateBins(bins, item, instance, itemToBin, alpha);

            Bin? selectedBin = null;

            if (candidateBins.Any())
            {
                selectedBin = candidateBins[random.Next(candidateBins.Count)];
            }
            else
            {
                var minBinRequired = GetMinimumBinIndex(item, instance, itemToBin);

                while (bins.Count <= minBinRequired)
                {
                    bins.Add(new Bin(bins.Count, instance.BinCapacity));
                }

                for (int i = minBinRequired; i < bins.Count; i++)
                {
                    if (bins[i].CanFit(item))
                    {
                        selectedBin = bins[i];
                        break;
                    }
                }
            }

            if (selectedBin == null)
            {
                selectedBin = new Bin(bins.Count, instance.BinCapacity);
                bins.Add(selectedBin);
            }

            selectedBin.AddItem(item);
            itemToBin[item.Id] = selectedBin.Id;
        }

        foreach (var bin in bins)
        {
            solution.AddBin(bin);
        }

        solution.ValidateSolution();

        return solution;
    }

    private int GetMinimumBinIndex(Item item, BppInstance instance, Dictionary<int, int> itemToBin)
    {
        int minBin = 0;

        foreach (var precedence in instance.Precedences)
        {
            if (precedence.ItemA == item.Id)
            {
                if (itemToBin.TryGetValue(precedence.ItemB, out var binB))
                {
                    minBin = Math.Max(minBin, binB + precedence.MinSeparation);
                }
            }
        }

        return minBin;
    }

    private List<Bin> GetCandidateBins(
        List<Bin> bins,
        Item item,
        BppInstance instance,
        Dictionary<int, int> itemToBin,
        double alpha)
    {
        var feasibleBins = bins
            .Where(b => b.CanFit(item) && IsPrecedenceFeasible(b, item, instance, itemToBin))
            .OrderBy(b => b.RemainingCapacity - item.Weight)
            .ToList();

        if (!feasibleBins.Any())
            return new List<Bin>();

        var minWaste = feasibleBins.First().RemainingCapacity - item.Weight;
        var maxWaste = feasibleBins.Last().RemainingCapacity - item.Weight;

        var threshold = minWaste + alpha * (maxWaste - minWaste);

        return feasibleBins
            .Where(b => (b.RemainingCapacity - item.Weight) <= threshold)
            .ToList();
    }

    private bool IsPrecedenceFeasible(
        Bin bin,
        Item item,
        BppInstance instance,
        Dictionary<int, int> itemToBin)
    {
        foreach (var precedence in instance.Precedences)
        {
            if (precedence.ItemA == item.Id)
            {
                if (itemToBin.TryGetValue(precedence.ItemB, out var binB))
                {
                    if (!precedence.IsSatisfied(bin.Id, binB))
                        return false;
                }
            }

            if (precedence.ItemB == item.Id)
            {
                if (itemToBin.TryGetValue(precedence.ItemA, out var binA))
                {
                    if (!precedence.IsSatisfied(binA, bin.Id))
                        return false;
                }
            }
        }

        return true;
    }
}
