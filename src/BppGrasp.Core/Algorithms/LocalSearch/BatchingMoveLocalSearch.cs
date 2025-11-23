using BppGrasp.Core.Models;
using BppGrasp.Core.Utils;

namespace BppGrasp.Core.Algorithms.LocalSearch;

public sealed class BatchingMoveLocalSearch
{
    private readonly ILogger _logger;

    public BatchingMoveLocalSearch(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public Solution Execute(Solution solution)
    {
        _logger.Log("[BatchingMoveLocalSearch] - [Execute] - Starting local search");

        var currentSolution = solution.Clone();
        var improved = true;
        var iteration = 0;

        while (improved)
        {
            iteration++;
            improved = false;

            var newSolution = TryMoveItems(currentSolution);

            if (newSolution.IsFeasible && newSolution.BinCount < currentSolution.BinCount)
            {
                currentSolution = newSolution;
                improved = true;
                _logger.Log($"[BatchingMoveLocalSearch] - [Execute] - Iteration {iteration}: Improved to {currentSolution.BinCount} bins");
            }

            if (!improved)
            {
                newSolution = TrySwapItems(currentSolution);

                if (newSolution.IsFeasible && newSolution.BinCount < currentSolution.BinCount)
                {
                    currentSolution = newSolution;
                    improved = true;
                    _logger.Log($"[BatchingMoveLocalSearch] - [Execute] - Iteration {iteration}: Improved to {currentSolution.BinCount} bins via swap");
                }
            }
        }

        _logger.Log($"[BatchingMoveLocalSearch] - [Execute] - Local search completed after {iteration} iterations");

        return currentSolution;
    }

    private Solution TryMoveItems(Solution solution)
    {
        var bestSolution = solution.Clone();

        for (int sourceBinIdx = 0; sourceBinIdx < solution.Bins.Count; sourceBinIdx++)
        {
            var sourceBin = solution.Bins[sourceBinIdx];

            foreach (var item in sourceBin.Items.ToList())
            {
                for (int targetBinIdx = 0; targetBinIdx < solution.Bins.Count; targetBinIdx++)
                {
                    if (sourceBinIdx == targetBinIdx)
                        continue;

                    var targetBin = solution.Bins[targetBinIdx];

                    if (CanMoveItem(item, sourceBin, targetBin, solution))
                    {
                        var newSolution = ApplyMove(solution, item, sourceBin.Id, targetBin.Id);

                        if (newSolution.IsFeasible && newSolution.BinCount <= bestSolution.BinCount)
                        {
                            bestSolution = newSolution;
                        }
                    }
                }
            }
        }

        return bestSolution;
    }

    private Solution TrySwapItems(Solution solution)
    {
        var bestSolution = solution.Clone();

        for (int bin1Idx = 0; bin1Idx < solution.Bins.Count; bin1Idx++)
        {
            for (int bin2Idx = bin1Idx + 1; bin2Idx < solution.Bins.Count; bin2Idx++)
            {
                var bin1 = solution.Bins[bin1Idx];
                var bin2 = solution.Bins[bin2Idx];

                foreach (var item1 in bin1.Items.ToList())
                {
                    foreach (var item2 in bin2.Items.ToList())
                    {
                        if (CanSwapItems(item1, item2, bin1, bin2, solution))
                        {
                            var newSolution = ApplySwap(solution, item1, item2, bin1.Id, bin2.Id);

                            if (newSolution.IsFeasible && newSolution.BinCount <= bestSolution.BinCount)
                            {
                                bestSolution = newSolution;
                            }
                        }
                    }
                }
            }
        }

        return bestSolution;
    }

    private bool CanMoveItem(Item item, Bin sourceBin, Bin targetBin, Solution solution)
    {
        if (!targetBin.CanFit(item))
            return false;

        var tempRemainingCapacity = sourceBin.RemainingCapacity + item.Weight;
        var tempUsedCapacity = targetBin.UsedCapacity + item.Weight;

        if (tempUsedCapacity > targetBin.Capacity)
            return false;

        return CheckPrecedenceConstraintsForMove(item, sourceBin.Id, targetBin.Id, solution);
    }

    private bool CanSwapItems(Item item1, Item item2, Bin bin1, Bin bin2, Solution solution)
    {
        var capacity1AfterSwap = bin1.UsedCapacity - item1.Weight + item2.Weight;
        var capacity2AfterSwap = bin2.UsedCapacity - item2.Weight + item1.Weight;

        if (capacity1AfterSwap > bin1.Capacity || capacity2AfterSwap > bin2.Capacity)
            return false;

        return CheckPrecedenceConstraintsForSwap(item1, item2, bin1.Id, bin2.Id, solution);
    }

    private bool CheckPrecedenceConstraintsForMove(Item item, int sourceBinId, int targetBinId, Solution solution)
    {
        foreach (var precedence in solution.Instance.Precedences)
        {
            if (precedence.ItemA == item.Id)
            {
                var binB = solution.GetBinForItem(precedence.ItemB);
                if (binB != -1 && !precedence.IsSatisfied(targetBinId, binB))
                    return false;
            }

            if (precedence.ItemB == item.Id)
            {
                var binA = solution.GetBinForItem(precedence.ItemA);
                if (binA != -1 && !precedence.IsSatisfied(binA, targetBinId))
                    return false;
            }
        }

        return true;
    }

    private bool CheckPrecedenceConstraintsForSwap(Item item1, Item item2, int bin1Id, int bin2Id, Solution solution)
    {
        return CheckPrecedenceConstraintsForMove(item1, bin1Id, bin2Id, solution) &&
               CheckPrecedenceConstraintsForMove(item2, bin2Id, bin1Id, solution);
    }

    private Solution ApplyMove(Solution solution, Item item, int sourceBinId, int targetBinId)
    {
        var newSolution = solution.Clone();

        var sourceBin = newSolution.Bins.First(b => b.Id == sourceBinId);
        var targetBin = newSolution.Bins.First(b => b.Id == targetBinId);

        sourceBin.RemoveItem(item);
        targetBin.AddItem(item);

        newSolution.ValidateSolution();

        return newSolution;
    }

    private Solution ApplySwap(Solution solution, Item item1, Item item2, int bin1Id, int bin2Id)
    {
        var newSolution = solution.Clone();

        var bin1 = newSolution.Bins.First(b => b.Id == bin1Id);
        var bin2 = newSolution.Bins.First(b => b.Id == bin2Id);

        bin1.RemoveItem(item1);
        bin2.RemoveItem(item2);

        bin1.AddItem(item2);
        bin2.AddItem(item1);

        newSolution.ValidateSolution();

        return newSolution;
    }
}
