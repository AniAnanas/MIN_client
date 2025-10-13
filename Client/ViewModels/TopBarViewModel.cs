using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class TopBarViewModel : INotifyPropertyChanged
    {
        private readonly WindowViewModel _windowViewModel;
        private string _searchText = string.Empty;
        private bool _isSearchVisible = false;

        public TopBarViewModel(WindowViewModel windowViewModel)
        {
            _windowViewModel = windowViewModel ?? throw new ArgumentNullException(nameof(windowViewModel));

            SearchCommand = new RelayCommand(_ => ToggleSearch());
            SettingsCommand = new RelayCommand(_ => OpenSettings());
            UserProfileCommand = new RelayCommand(_ => OpenUserProfile());
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged();
                    // Trigger search logic here
                }
            }
        }

        public bool IsSearchVisible
        {
            get => _isSearchVisible;
            set
            {
                if (_isSearchVisible != value)
                {
                    _isSearchVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public string WindowTitle => _windowViewModel.Title;

        public WindowState CurrentWindowState => _windowViewModel.WindowState;

        public ICommand SearchCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand UserProfileCommand { get; }
        public ICommand MinimizeCommand => _windowViewModel.MinimizeCommand;
        public ICommand MaximizeCommand => _windowViewModel.MaximizeCommand;
        public ICommand CloseCommand => _windowViewModel.CloseCommand;
        
        private void ToggleSearch()
        {
            IsSearchVisible = !IsSearchVisible;
        }

        private void OpenSettings()
        {
            // TODO: Implement settings navigation
            Log.Info("Settings command executed");
        }

        private void OpenUserProfile()
        {
            // TODO: Implement user profile navigation
            Log.Info("User profile command executed");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
