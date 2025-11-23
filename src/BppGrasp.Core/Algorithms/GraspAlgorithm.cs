using BppGrasp.Core.Models;
using BppGrasp.Core.Utils;
using BppGrasp.Core.Algorithms.ConstructiveHeuristics;
using BppGrasp.Core.Algorithms.LocalSearch;

namespace BppGrasp.Core.Algorithms;

public sealed class GraspAlgorithm
{
    private readonly ILogger _logger;
    private readonly PreProcessing _preProcessing;
    private readonly BoundsCalculator _boundsCalculator;
    private readonly List<IConstructiveHeuristic> _constructiveHeuristics;
    private readonly BatchingMoveLocalSearch _localSearch;
    private readonly Random _random;

    public GraspAlgorithm(ILogger logger, int seed = 42)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _preProcessing = new PreProcessing(logger);
        _boundsCalculator = new BoundsCalculator(logger);
        _localSearch = new BatchingMoveLocalSearch(logger);
        _random = new Random(seed);

        _constructiveHeuristics = new List<IConstructiveHeuristic>
        {
            new FirstFitDecreasingHeuristic(logger),
            new BestFitDecreasingHeuristic(logger)
        };
    }

    public Solution Solve(BppInstance instance, GraspParameters parameters)
    {
        _logger.Log("[GraspAlgorithm] - [Solve] - Starting GRASP algorithm");
        _logger.LogProgress("\n🔍 Starting GRASP Algorithm...");

        var improvedInstance = _preProcessing.Execute(instance);
        var lowerBound = _boundsCalculator.ComputeLowerBound(improvedInstance);

        _logger.Log($"[GraspAlgorithm] - [Solve] - Lower bound: {lowerBound}");
        _logger.LogProgress($"   Lower Bound: {lowerBound} bins");

        Solution? bestSolution = null;
        var startTime = DateTime.Now;
        var iteration = 0;

        while (!IsStoppingCriteriaMet(startTime, parameters.MaxTime, iteration, parameters.MaxIterations))
        {
            iteration++;

            var alpha = GetAdaptiveAlpha(iteration, parameters);
            var currentSolution = ConstructInitialSolution(improvedInstance, alpha);

            if (currentSolution.IsFeasible)
            {
                currentSolution = _localSearch.Execute(currentSolution);

                if (bestSolution == null || (currentSolution.IsFeasible && currentSolution.BinCount < bestSolution.BinCount))
                {
                    bestSolution = currentSolution.Clone();
                    _logger.Log($"[GraspAlgorithm] - [Solve] - Iteration {iteration}: New best solution with {bestSolution.BinCount} bins (Gap: {bestSolution.BinCount - lowerBound})");
                    _logger.LogProgress($"   Iteration {iteration,3}: ✓ New best = {bestSolution.BinCount} bins (gap: {bestSolution.BinCount - lowerBound})");

                    if (bestSolution.BinCount == lowerBound)
                    {
                        _logger.Log("[GraspAlgorithm] - [Solve] - Optimal solution found!");
                        _logger.LogResult($"   🎯 Optimal solution found!");
                        break;
                    }
                }
            }

            if (iteration % 25 == 0 && bestSolution != null)
            {
                _logger.Log($"[GraspAlgorithm] - [Solve] - Iteration {iteration}: Best={bestSolution?.BinCount ?? -1}, Current={currentSolution.BinCount}");
                _logger.LogProgress($"   Iteration {iteration,3}: Best = {bestSolution.BinCount} bins");
            }
        }

        var elapsedTime = DateTime.Now - startTime;
        _logger.Log($"[GraspAlgorithm] - [Solve] - GRASP completed in {elapsedTime.TotalSeconds:F2}s after {iteration} iterations");
        _logger.Log($"[GraspAlgorithm] - [Solve] - Best solution: {bestSolution?.BinCount ?? -1} bins (LB: {lowerBound})");

        _logger.LogProgress($"\n✅ GRASP Completed:");
        _logger.LogProgress($"   Total Iterations: {iteration}");
        _logger.LogProgress($"   Time Elapsed: {elapsedTime.TotalSeconds:F2}s");
        _logger.LogProgress($"   Best Solution: {bestSolution?.BinCount ?? -1} bins");

        return bestSolution ?? new Solution(improvedInstance);
    }

    private Solution ConstructInitialSolution(BppInstance instance, double alpha)
    {
        var solutions = new List<Solution>();

        foreach (var heuristic in _constructiveHeuristics)
        {
            var solution = heuristic.Construct(instance, alpha, _random);
            if (solution.IsFeasible)
            {
                solutions.Add(solution);
            }
        }

        if (!solutions.Any())
        {
            _logger.LogError("[GraspAlgorithm] - [ConstructInitialSolution] - No feasible solution found");
            return new Solution(instance);
        }

        return solutions.OrderBy(s => s.BinCount).First();
    }

    private double GetAdaptiveAlpha(int iteration, GraspParameters parameters)
    {
        if (!parameters.UseAdaptiveAlpha)
            return parameters.Alpha;

        var progress = Math.Min(1.0, (double)iteration / parameters.MaxIterations);

        return parameters.AlphaMin + (parameters.AlphaMax - parameters.AlphaMin) * (1 - progress);
    }

    private bool IsStoppingCriteriaMet(DateTime startTime, TimeSpan maxTime, int iteration, int maxIterations)
    {
        var elapsed = DateTime.Now - startTime;

        if (elapsed >= maxTime)
            return true;

        if (iteration >= maxIterations)
            return true;

        return false;
    }
}

public sealed class GraspParameters
{
    public double Alpha { get; set; } = 0.3;
    public bool UseAdaptiveAlpha { get; set; } = true;
    public double AlphaMin { get; set; } = 0.1;
    public double AlphaMax { get; set; } = 0.5;
    public TimeSpan MaxTime { get; set; } = TimeSpan.FromMinutes(5);
    public int MaxIterations { get; set; } = 1000;

    public static GraspParameters Default => new GraspParameters();
}
