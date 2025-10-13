using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Client.ViewModels
{
    public enum ViewType
    {
        ChatList,
        Chat,
        Settings,
        UserProfile,
        Auth,
        Loading
    }

    public class NavigationViewModel : INotifyPropertyChanged
    {
        private ViewType _currentView = ViewType.ChatList;
        private object? _currentViewModel;

        public NavigationViewModel()
        {
            NavigateToChatListCommand = new RelayCommand(_ => NavigateToChatList());
            NavigateToSettingsCommand = new RelayCommand(_ => NavigateToSettings());
            NavigateToUserProfileCommand = new RelayCommand(_ => NavigateToUserProfile());
            NavigateToAuthCommand = new RelayCommand(_ => NavigateToAuth());
        }

        public ViewType CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                    UpdateCurrentViewModel();
                }
            }
        }

        public object? CurrentViewModel
        {
            get => _currentViewModel;
            set
            {
                if (_currentViewModel != value)
                {
                    _currentViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NavigateToChatListCommand { get; }
        public ICommand NavigateToSettingsCommand { get; }
        public ICommand NavigateToUserProfileCommand { get; }
        public ICommand NavigateToAuthCommand { get; }

        private void NavigateToChatList()
        {
            CurrentView = ViewType.ChatList;
            Log.Info("Navigated to Chat List");
        }

        private void NavigateToSettings()
        {
            CurrentView = ViewType.Settings;
            Log.Info("Navigated to Settings");
        }

        private void NavigateToUserProfile()
        {
            CurrentView = ViewType.UserProfile;
            Log.Info("Navigated to User Profile");
        }

        private void NavigateToAuth()
        {
            CurrentView = ViewType.Auth;
            Log.Info("Navigated to Auth");
        }

        private void UpdateCurrentViewModel()
        {
            // TODO: Implement view model creation based on view type
            // This would typically use a service locator or dependency injection
            switch (CurrentView)
            {
                case ViewType.ChatList:
                    CurrentViewModel = new ChatListViewModel();
                    break;
                //case ViewType.Settings:
                //    CurrentViewModel = new SettingsViewModel();
                //    break;
                //case ViewType.UserProfile:
                //    CurrentViewModel = new UserProfileViewModel();
                //    break;
                //case ViewType.Auth:
                //    CurrentViewModel = new AuthViewModel();
                //    break;
                default:
                    CurrentViewModel = null;
                    break;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
