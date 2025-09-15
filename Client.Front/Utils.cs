using Client.Front;
using Client.Front.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Front
{
    public abstract class Log
    {
        private static readonly ConsoleColor NameColor = ConsoleColor.Cyan;
        private static object _lock = new();
        private static string LogPath = Path.Combine(MainWindow.Instance?.currentDir ?? Directory.CreateTempSubdirectory().FullName, "Logs");
        private static void CreateFolder()
        {
            try
            {
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = NameColor;
                Console.Write($"[Log System] ");
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
        }

        public static void LogFile(string message)
        {
            try
            {
                lock (_lock)
                {
                    string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH:mm:ss");
                    CreateFolder();
                    string lastBoot = MainWindow.Instance?.timeBoot ?? DateTime.Now.ToString("yyyy-MM-dd");
                    var stackTrace = new System.Diagnostics.StackTrace();
                    var frame = stackTrace.GetFrame(1)?.GetMethod();
                    var methodName = frame?.Name ?? "Unknown";
                    var className = frame?.ReflectedType?.Name ?? "Unknown";
                    if (methodName != "General" && methodName != "Info" && methodName != "Error" && methodName != "Success" && methodName != "Warn") methodName = "Unknown";

                    string msg = timestamp + $" [{methodName.ToUpper().First()}]" + $" ({className})" + " => " + message + Environment.NewLine;

                    //File.AppendAllText(Path.Combine(TShock.SavePath, "Zeronomi", "Zeronomi.log"), msg);
                    File.AppendAllText(Path.Combine(LogPath, $"{lastBoot}.log"), msg);
                }
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(ex.ToString());
                Console.ResetColor();
            }
        }

        public static void Info(string message)
        {
            CreateFolder();
            Console.ForegroundColor = NameColor;
            Console.Write($"[Log System] ");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
            LogFile(message);
        }

        public static void Error(string message)
        {
            CreateFolder();
            Console.ForegroundColor = NameColor;
            Console.Write($"[Log System] ");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
            LogFile(message);
        }

        public static void Success(string message)
        {
            CreateFolder();
            Console.ForegroundColor = NameColor;
            Console.Write($"[Log System] ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message);
            Console.ResetColor();
            LogFile(message);
        }

        public static void Warn(string message)
        {
            CreateFolder();
            Console.ForegroundColor = NameColor;
            Console.Write($"[Log System] ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
            LogFile(message);
        }

        public static void General(string message)
        {
            CreateFolder();
            Console.ForegroundColor = NameColor;
            Console.Write($"[Log System] ");
            Console.ResetColor();
            Console.WriteLine(message);
            LogFile(message);
        }
    }
}
