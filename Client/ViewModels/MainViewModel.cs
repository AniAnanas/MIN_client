using Client.Services;
using Client.Services.Interfaces;
using System.ComponentModel;
using System.Windows;

namespace Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ViewModels
        public WindowViewModel WindowViewModel { get; }
        public TopBarViewModel TopBarViewModel { get; }
        public ChatListViewModel ChatListViewModel { get; }
        public ChatViewModel ChatViewModel { get; }
        public NavigationViewModel NavigationViewModel { get; }

        // Services
        private readonly ILoggingService _loggingService;
        private readonly INavigationService _navigationService;

        public MainViewModel()
        {
            // Initialize services
            _loggingService = new LoggingService();
            _navigationService = new NavigationService();

            // Initialize ViewModels
            WindowViewModel = new WindowViewModel();
            TopBarViewModel = new TopBarViewModel(WindowViewModel);
            ChatListViewModel = new ChatListViewModel();
            NavigationViewModel = new NavigationViewModel();

            // Set up navigation
            NavigationViewModel.CurrentViewModel = ChatListViewModel;

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
