using BppGrasp.Core.Models;
using BppGrasp.Core.Utils;
using BppGrasp.Core.Algorithms.ConstructiveHeuristics;

namespace BppGrasp.Core.Algorithms.LocalSearch;

public sealed class DestroyShrinkRepair
{
    private readonly ILogger _logger;
    private readonly IConstructiveHeuristic _constructiveHeuristic;

    public DestroyShrinkRepair(ILogger logger, IConstructiveHeuristic constructiveHeuristic)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _constructiveHeuristic = constructiveHeuristic ?? throw new ArgumentNullException(nameof(constructiveHeuristic));
    }

    public Solution Execute(Solution solution, int rho, Random random)
    {
        _logger.Log($"[DestroyShrinkRepair] - [Execute] - Destroying {rho} bins");

        if (solution.BinCount <= rho)
        {
            return solution;
        }

        var binsToRemove = SelectBinsToDestroy(solution, rho, random);
        var itemsToReinsert = new List<Item>();

        foreach (var bin in binsToRemove)
        {
            itemsToReinsert.AddRange(bin.Items);
        }

        var partialSolution = CreatePartialSolution(solution, binsToRemove);

        var repairedSolution = RepairSolution(partialSolution, itemsToReinsert, random);

        _logger.Log($"[DestroyShrinkRepair] - [Execute] - Repair completed: {repairedSolution.BinCount} bins");

        return repairedSolution;
    }

    private List<Bin> SelectBinsToDestroy(Solution solution, int rho, Random random)
    {
        var binsByUtilization = solution.Bins
            .OrderBy(b => b.UsedCapacity / b.Capacity)
            .Take(Math.Min(rho, solution.BinCount))
            .ToList();

        return binsByUtilization;
    }

    private Solution CreatePartialSolution(Solution solution, List<Bin> binsToRemove)
    {
        var partialSolution = new Solution(solution.Instance);

        var binsToKeep = solution.Bins
            .Where(b => !binsToRemove.Contains(b))
            .ToList();

        foreach (var bin in binsToKeep)
        {
            partialSolution.AddBin(bin.Clone());
        }

        return partialSolution;
    }

    private Solution RepairSolution(Solution partialSolution, List<Item> itemsToReinsert, Random random)
    {
        var itemsOrdered = itemsToReinsert.OrderByDescending(i => i.Weight).ToList();

        foreach (var item in itemsOrdered)
        {
            var inserted = false;

            foreach (var bin in partialSolution.Bins)
            {
                if (bin.CanFit(item) && IsPrecedenceFeasible(bin, item, partialSolution))
                {
                    bin.AddItem(item);
                    inserted = true;
                    break;
                }
            }

            if (!inserted)
            {
                var newBin = new Bin(partialSolution.BinCount, partialSolution.Instance.BinCapacity);
                newBin.AddItem(item);
                partialSolution.AddBin(newBin);
            }
        }

        partialSolution.ValidateSolution();
        return partialSolution;
    }

    private bool IsPrecedenceFeasible(Bin bin, Item item, Solution solution)
    {
        foreach (var precedence in solution.Instance.Precedences)
        {
            if (precedence.ItemA == item.Id)
            {
                var binB = solution.GetBinForItem(precedence.ItemB);
                if (binB != -1 && !precedence.IsSatisfied(bin.Id, binB))
                    return false;
            }

            if (precedence.ItemB == item.Id)
            {
                var binA = solution.GetBinForItem(precedence.ItemA);
                if (binA != -1 && !precedence.IsSatisfied(binA, bin.Id))
                    return false;
            }
        }

        return true;
    }
}
