using BppGrasp.Core.Models;
using BppGrasp.Core.Utils;

namespace BppGrasp.Core.Algorithms;

public sealed class BoundsCalculator
{
    private readonly ILogger _logger;

    public BoundsCalculator(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public int ComputeLowerBound(BppInstance instance)
    {
        _logger.Log("[BoundsCalculator] - [ComputeLowerBound] - Computing lower bound");

        var l0 = ComputeL0(instance);
        var l1 = ComputeL1(instance);
        var l2 = ComputeL2(instance);

        var lowerBound = Math.Max(l0, Math.Max(l1, l2));

        _logger.Log($"[BoundsCalculator] - [ComputeLowerBound] - L0={l0}, L1={l1}, L2={l2}, LB={lowerBound}");

        return lowerBound;
    }

    private int ComputeL0(BppInstance instance)
    {
        var totalWeight = instance.Items.Sum(i => i.Weight);
        return (int)Math.Ceiling(totalWeight / instance.BinCapacity);
    }

    private int ComputeL1(BppInstance instance)
    {
        if (instance.Precedences.Count == 0)
            return 0;

        var maxChainLength = ComputeLongestChain(instance);
        return maxChainLength;
    }

    private int ComputeLongestChain(BppInstance instance)
    {
        var graph = new Dictionary<int, List<int>>();

        foreach (var item in instance.Items)
        {
            graph[item.Id] = new List<int>();
        }

        foreach (var precedence in instance.Precedences)
        {
            if (graph.ContainsKey(precedence.ItemB))
            {
                graph[precedence.ItemB].Add(precedence.ItemA);
            }
        }

        var memo = new Dictionary<int, int>();

        int DFS(int node)
        {
            if (memo.ContainsKey(node))
                return memo[node];

            int maxLength = 1;

            if (graph.ContainsKey(node))
            {
                foreach (var neighbor in graph[node])
                {
                    maxLength = Math.Max(maxLength, 1 + DFS(neighbor));
                }
            }

            memo[node] = maxLength;
            return maxLength;
        }

        var longestChain = 0;
        foreach (var item in instance.Items)
        {
            longestChain = Math.Max(longestChain, DFS(item.Id));
        }

        return longestChain;
    }

    private int ComputeL2(BppInstance instance)
    {
        var itemsByWeight = instance.Items.OrderByDescending(i => i.Weight).ToList();

        var bins = new List<double>();

        foreach (var item in itemsByWeight)
        {
            var placed = false;

            for (int i = 0; i < bins.Count; i++)
            {
                if (bins[i] + item.Weight <= instance.BinCapacity)
                {
                    bins[i] += item.Weight;
                    placed = true;
                    break;
                }
            }

            if (!placed)
            {
                bins.Add(item.Weight);
            }
        }

        return bins.Count;
    }
}
