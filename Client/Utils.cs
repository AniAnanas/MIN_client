using Client.Shared.Interfaces;

namespace Client
{
    // Static wrapper for backward compatibility
    public static class Log
    {
        private static ILoggingService? _logger;

        public static void Initialize(ILoggingService logger)
        {
            _logger = logger;
        }

        public static void Info(string message)
        {
            _logger?.Info(message);
        }

        public static void Error(string message)
        {
            _logger?.Error(message);
        }

        public static void Warn(string message)
        {
            _logger?.Warn(message);
        }

        public static void Success(string message)
        {
            _logger?.Success(message);
        }

        public static void General(string message)
        {
            _logger?.General(message);
        }
    }
}
