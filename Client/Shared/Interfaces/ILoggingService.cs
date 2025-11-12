namespace Client.Shared.Interfaces
{
    public interface ILoggingService
    {
        void Info(string message);
        void Error(string message);
        void Warn(string message);
        void Success(string message);
        void General(string message);
    }
}
