using Client.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Services
{
    public class LoggingService : ILoggingService
    {
        private static readonly object _lock = new();
        private readonly string _logPath;

        public LoggingService()
        {
            _logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
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
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss");
                    string lastBoot = DateTime.Now.ToString("yyyy-MM-dd");
                    var stackTrace = new System.Diagnostics.StackTrace();
                    var frame = stackTrace.GetFrame(2)?.GetMethod(); // Go up two frames to get the actual caller
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
}
