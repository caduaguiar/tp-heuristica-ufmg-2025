namespace BppGrasp.Core.Utils;

public interface ILogger
{
    void Log(string message);
    void LogError(string message);
    void LogProgress(string message);
    void LogResult(string message);
}

public sealed class ConsoleLogger : ILogger
{
    private readonly bool _enabled;
    private readonly bool _verbose;

    public ConsoleLogger(bool enabled = true, bool verbose = false)
    {
        _enabled = enabled;
        _verbose = verbose;
    }

    public void Log(string message)
    {
        if (_enabled && _verbose)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }

    public void LogError(string message)
    {
        if (_enabled)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {message}");
            Console.ForegroundColor = previousColor;
        }
    }

    public void LogProgress(string message)
    {
        if (_enabled)
        {
            Console.WriteLine(message);
        }
    }

    public void LogResult(string message)
    {
        if (_enabled)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
        }
    }
}
