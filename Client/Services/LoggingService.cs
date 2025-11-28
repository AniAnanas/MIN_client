using Client.Shared.Interfaces;
using System.IO;

namespace Client.Services;

public class LoggingService : ILoggingService
{
    private static readonly Lock _lock = new();
    private readonly string _logPath;

    public LoggingService()
    {
        _logPath = Path.Combine(Environment.CurrentDirectory, "Logs");
        CreateFolder();
    }

    private void CreateFolder()
    {
        try
        {
            if (!Directory.Exists(_logPath))
            {
                Directory.CreateDirectory(_logPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to create log directory: {ex.Message}");
        }
    }

    private void LogToFile(string level, string message)
    {
        try
        {
            lock (_lock)
            {
                if (App.Current == null) return;

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss");
                string lastBoot = ((App)App.Current).StartTime.ToString("HH-mm-ss_dd-MM-yyyy");
                var stackTrace = new System.Diagnostics.StackTrace();
                var frame = stackTrace.GetFrame(3)?.GetMethod(); // Go up 3 frames to get the actual caller
                var methodName = frame?.Name ?? "Unknown";
                var className = frame?.ReflectedType?.Name ?? "Unknown";

                string logMessage = $"{timestamp} [{level.ToUpper().First()}] ({className}.{methodName}) => {message}{Environment.NewLine}";

                File.AppendAllText(Path.Combine(_logPath, $"{lastBoot}.log"), logMessage);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to write to log file: {ex.Message}");
        }
    }

    private void LogToConsole(string level, string message, ConsoleColor color)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"[Log System] ");
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    public void Info(string message)
    {
        LogToConsole("INFO", message, ConsoleColor.Cyan);
        LogToFile("INFO", message);
    }

    public void Error(string message)
    {
        LogToConsole("ERROR", message, ConsoleColor.Red);
        LogToFile("ERROR", message);
    }

    public void Warn(string message)
    {
        LogToConsole("WARN", message, ConsoleColor.Yellow);
        LogToFile("WARN", message);
    }

    public void Success(string message)
    {
        LogToConsole("SUCCESS", message, ConsoleColor.Green);
        LogToFile("SUCCESS", message);
    }

    public void General(string message)
    {
        LogToConsole("GENERAL", message, ConsoleColor.White);
        LogToFile("GENERAL", message);
    }
}
