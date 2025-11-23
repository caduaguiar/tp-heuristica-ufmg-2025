using BppGrasp.Core.Algorithms;
using BppGrasp.Core.IO;
using BppGrasp.Core.Models;
using BppGrasp.Core.Utils;

namespace BppGrasp.Console;

public class Program
{
    public static void Main(string[] args)
    {
        PrintHeader();

        var verbose = args.Contains("--verbose") || args.Contains("-v");
        var logger = new ConsoleLogger(enabled: true, verbose: verbose);

        try
        { 
            var filePath = args.FirstOrDefault(a => !a.StartsWith("-"));

            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                RunFromFile(filePath, logger);
            }
            else if (!string.IsNullOrEmpty(filePath))
            {
                logger.LogError($"File not found: {filePath}");
            }
            else
            {
                RunExamples(logger);
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"An error occurred: {ex.Message}");
            if (verbose)
            {
                logger.LogError(ex.StackTrace ?? "");
            }
        }

        if (System.Console.IsInputRedirected == false)
        {
            System.Console.WriteLine("\nPress any key to exit...");
            System.Console.ReadKey();
        }
    }

    private static void PrintHeader()
    {
        System.Console.ForegroundColor = ConsoleColor.Cyan;
        System.Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        System.Console.WriteLine("║         Bin Packing Problem with General Precedence           ║");
        System.Console.WriteLine("║                    GRASP Metaheuristic                         ║");
        System.Console.WriteLine("║                                                                ║");
        System.Console.WriteLine("║              UFMG - Departamento de Computação                 ║");
        System.Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        System.Console.ResetColor();
        System.Console.WriteLine();
    }

    private static void RunFromFile(string filePath, ILogger logger)
    {
        logger.LogProgress($"\n📂 Reading instance from: {filePath}");

        var instance = InstanceReader.ReadFromFile(filePath);

        logger.LogProgress($"   Instance: {instance.Name}");
        logger.LogProgress($"   Items: {instance.Items.Count}");
        logger.LogProgress($"   Bin Capacity: {instance.BinCapacity}");
        logger.LogProgress($"   Precedences: {instance.Precedences.Count}");

        SolveInstance(instance, logger);
    }

    private static void RunExamples(ILogger logger)
    {
        logger.LogProgress("No input file provided. Running example instances...\n");

        RunSimpleExample(logger);
        System.Console.WriteLine("\n" + new string('=', 80) + "\n");

        RunSmallRandomExample(logger);
        System.Console.WriteLine("\n" + new string('=', 80) + "\n");

        RunMediumRandomExample(logger);
    }

    private static void RunSimpleExample(ILogger logger)
    {
        logger.LogProgress("📦 EXAMPLE 1: Simple Instance");
        logger.LogProgress("   " + new string('-', 40));

        var generator = new InstanceGenerator(42);
        var instance = generator.GenerateSimpleInstance();

        logger.LogProgress($"   Items: {instance.Items.Count}, Capacity: {instance.BinCapacity}, Precedences: {instance.Precedences.Count}");

        SolveInstance(instance, logger);
    }

    private static void RunSmallRandomExample(ILogger logger)
    {
        logger.LogProgress("📦 EXAMPLE 2: Small Random Instance");
        logger.LogProgress("   " + new string('-', 40));

        var generator = new InstanceGenerator(42);
        var instance = generator.GenerateRandomInstance(
            name: "Small_Random",
            itemCount: 20,
            binCapacity: 100.0,
            minWeight: 10.0,
            maxWeight: 50.0,
            precedenceCount: 10
        );

        logger.LogProgress($"   Items: {instance.Items.Count}, Capacity: {instance.BinCapacity}, Precedences: {instance.Precedences.Count}");

        SolveInstance(instance, logger);
    }

    private static void RunMediumRandomExample(ILogger logger)
    {
        logger.LogProgress("📦 EXAMPLE 3: Medium Random Instance");
        logger.LogProgress("   " + new string('-', 40));

        var generator = new InstanceGenerator(123);
        var instance = generator.GenerateRandomInstance(
            name: "Medium_Random",
            itemCount: 50,
            binCapacity: 150.0,
            minWeight: 15.0,
            maxWeight: 60.0,
            precedenceCount: 25
        );

        logger.LogProgress($"   Items: {instance.Items.Count}, Capacity: {instance.BinCapacity}, Precedences: {instance.Precedences.Count}");

        SolveInstance(instance, logger);
    }

    private static void SolveInstance(BppInstance instance, ILogger logger)
    {
        var parameters = new GraspParameters
        {
            Alpha = 0.3,
            UseAdaptiveAlpha = true,
            AlphaMin = 0.1,
            AlphaMax = 0.5,
            MaxTime = TimeSpan.FromSeconds(30),
            MaxIterations = 100
        };

        var algorithm = new GraspAlgorithm(logger, seed: 42);

        var solution = algorithm.Solve(instance, parameters);

        PrintSolution(solution, logger);
    }

    private static void PrintInstanceDetails(BppInstance instance, ILogger logger)
    {
        logger.Log("\nItems:");
        foreach (var item in instance.Items.Take(10))
        {
            logger.Log($"  - {item}");
        }

        if (instance.Items.Count > 10)
        {
            logger.Log($"  ... and {instance.Items.Count - 10} more items");
        }

        logger.Log("\nPrecedence Constraints:");
        foreach (var prec in instance.Precedences.Take(10))
        {
            logger.Log($"  - {prec}");
        }

        if (instance.Precedences.Count > 10)
        {
            logger.Log($"  ... and {instance.Precedences.Count - 10} more constraints");
        }

        logger.Log("");
    }

    private static void PrintSolution(Solution solution, ILogger logger)
    {
        logger.LogProgress("\n" + new string('=', 50));
        logger.LogProgress("📊 SOLUTION SUMMARY");
        logger.LogProgress(new string('=', 50));

        logger.LogProgress($"Instance: {solution.Instance.Name}");
        logger.LogProgress($"Items: {solution.Instance.Items.Count} | Bin Capacity: {solution.Instance.BinCapacity} | Precedences: {solution.Instance.Precedences.Count}");

        logger.LogProgress($"\n{"Result",-20}: {(solution.IsFeasible ? "✓ FEASIBLE" : "✗ INFEASIBLE")}");
        logger.LogResult($"{"Bins Used",-20}: {solution.BinCount}");

        if (solution.IsFeasible && solution.Bins.Count > 0)
        {
            var avgUtilization = solution.Bins.Average(b => (b.UsedCapacity / b.Capacity) * 100);
            logger.LogProgress($"{"Avg Utilization",-20}: {avgUtilization:F1}%");

            logger.LogProgress($"\nBin Details (showing {Math.Min(5, solution.Bins.Count)} of {solution.Bins.Count}):");

            var binsToShow = Math.Min(5, solution.Bins.Count);

            for (int i = 0; i < binsToShow; i++)
            {
                var bin = solution.Bins[i];
                var utilization = (bin.UsedCapacity / bin.Capacity) * 100;
                var bar = GenerateBar(utilization);
                logger.LogProgress($"  Bin {bin.Id,2}: {bar} {utilization,5:F1}% ({bin.UsedCapacity,6:F1}/{bin.Capacity,6:F1}) - {bin.Items.Count} items");
            }

            if (solution.Bins.Count > 5)
            {
                logger.LogProgress($"  ... and {solution.Bins.Count - 5} more bins");
            }
        }

        logger.LogProgress(new string('=', 50) + "\n");
    }

    private static string GenerateBar(double percentage)
    {
        int barLength = 20;
        int filled = (int)(percentage / 100.0 * barLength);
        return "[" + new string('█', filled) + new string('░', barLength - filled) + "]";
    }
}
