using BppGrasp.Core.Models;
using BppGrasp.Core.Utils;

namespace BppGrasp.Core.Algorithms;

public sealed class PreProcessing
{
    private readonly ILogger _logger;

    public PreProcessing(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public BppInstance Execute(BppInstance instance)
    {
        _logger.Log("[PreProcessing] - [Execute] - Starting preprocessing");

        var processedInstance = instance.Clone();

        RemoveRedundantPrecedences(processedInstance);
        SortItemsByTopologicalOrder(processedInstance);

        _logger.Log($"[PreProcessing] - [Execute] - Preprocessing completed. Items: {processedInstance.Items.Count}, Precedences: {processedInstance.Precedences.Count}");

        return processedInstance;
    }

    private void RemoveRedundantPrecedences(BppInstance instance)
    {
        var graph = BuildPrecedenceGraph(instance);
        var transitiveClosure = ComputeTransitiveClosure(graph, instance.Items.Count);

        var redundant = new HashSet<PrecedenceConstraint>();

        foreach (var precedence in instance.Precedences)
        {
            var canReach = CanReachWithoutEdge(
                precedence.ItemA,
                precedence.ItemB,
                precedence,
                transitiveClosure,
                instance.Precedences);

            if (canReach)
            {
                redundant.Add(precedence);
            }
        }

        instance.Precedences.RemoveAll(p => redundant.Contains(p));

        _logger.Log($"[PreProcessing] - [RemoveRedundantPrecedences] - Removed {redundant.Count} redundant precedences");
    }

    private Dictionary<int, List<(int, int)>> BuildPrecedenceGraph(BppInstance instance)
    {
        var graph = new Dictionary<int, List<(int, int)>>();

        foreach (var item in instance.Items)
        {
            graph[item.Id] = new List<(int, int)>();
        }

        foreach (var precedence in instance.Precedences)
        {
            if (graph.ContainsKey(precedence.ItemB))
            {
                graph[precedence.ItemB].Add((precedence.ItemA, precedence.MinSeparation));
            }
        }

        return graph;
    }

    private bool[,] ComputeTransitiveClosure(Dictionary<int, List<(int, int)>> graph, int itemCount)
    {
        var closure = new bool[itemCount, itemCount];

        foreach (var kvp in graph)
        {
            foreach (var (successor, _) in kvp.Value)
            {
                if (kvp.Key < itemCount && successor < itemCount)
                {
                    closure[kvp.Key, successor] = true;
                }
            }
        }

        for (int k = 0; k < itemCount; k++)
        {
            for (int i = 0; i < itemCount; i++)
            {
                for (int j = 0; j < itemCount; j++)
                {
                    closure[i, j] = closure[i, j] || (closure[i, k] && closure[k, j]);
                }
            }
        }

        return closure;
    }

    private bool CanReachWithoutEdge(
        int from,
        int to,
        PrecedenceConstraint excludeEdge,
        bool[,] transitiveClosure,
        List<PrecedenceConstraint> precedences)
    {
        var visited = new HashSet<int>();
        var queue = new Queue<int>();
        queue.Enqueue(to);
        visited.Add(to);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (current == from)
                return true;

            foreach (var precedence in precedences)
            {
                if (precedence == excludeEdge)
                    continue;

                if (precedence.ItemB == current && !visited.Contains(precedence.ItemA))
                {
                    visited.Add(precedence.ItemA);
                    queue.Enqueue(precedence.ItemA);
                }
            }
        }

        return false;
    }

    private void SortItemsByTopologicalOrder(BppInstance instance)
    {
        var inDegree = new Dictionary<int, int>();
        var graph = new Dictionary<int, List<int>>();

        foreach (var item in instance.Items)
        {
            inDegree[item.Id] = 0;
            graph[item.Id] = new List<int>();
        }

        foreach (var precedence in instance.Precedences)
        {
            if (graph.ContainsKey(precedence.ItemB) && inDegree.ContainsKey(precedence.ItemA))
            {
                graph[precedence.ItemB].Add(precedence.ItemA);
                inDegree[precedence.ItemA]++;
            }
        }

        var queue = new Queue<int>();
        foreach (var item in instance.Items)
        {
            if (inDegree[item.Id] == 0)
            {
                queue.Enqueue(item.Id);
            }
        }

        var sortedIds = new List<int>();

        while (queue.Count > 0)
        {
            var itemId = queue.Dequeue();
            sortedIds.Add(itemId);

            if (graph.ContainsKey(itemId))
            {
                foreach (var successor in graph[itemId])
                {
                    inDegree[successor]--;
                    if (inDegree[successor] == 0)
                    {
                        queue.Enqueue(successor);
                    }
                }
            }
        }

        var itemMap = instance.Items.ToDictionary(i => i.Id);
        instance.Items.Clear();
        instance.Items.AddRange(sortedIds.Select(id => itemMap[id]));

        _logger.Log("[PreProcessing] - [SortItemsByTopologicalOrder] - Items sorted by topological order");
    }
}
