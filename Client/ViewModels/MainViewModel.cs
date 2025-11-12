using Client.Services;
using Client.Shared.Interfaces;
using System.ComponentModel;
using System.Windows;

namespace Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ViewModels
        public WindowViewModel WindowViewModel { get; }
        public TopBarViewModel TopBarViewModel { get; }
        public TabListViewModel ChatListViewModel { get; }
        public ChatViewModel ChatViewModel { get; }
        public TabListViewModel TabListViewModel { get; }

        public MainWindow Window { get; }

        // Services
        private readonly ILoggingService _loggingService;

        public MainViewModel(MainWindow window)
        {
            Window = window;

            // Initialize services
            _loggingService = new LoggingService();
            // Initialize ViewModels
            WindowViewModel = new WindowViewModel();
            TopBarViewModel = new TopBarViewModel(WindowViewModel);
            
            ChatListViewModel = new TabListViewModel();
            TabListViewModel = new TabListViewModel(); // Инициализация TabListViewModel

            // Initialize logging
            Log.Initialize(_loggingService);

            Log.Info("MainViewModel initialized successfully");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
