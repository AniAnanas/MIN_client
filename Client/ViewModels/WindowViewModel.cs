using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class WindowViewModel : INotifyPropertyChanged
    {
        private WindowState _windowState = WindowState.Normal;
        private string _title = "Messenger";

        public WindowState WindowState
        {
            get => _windowState;
            set
            {
                if (_windowState != value)
                {
                    _windowState = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand DragMoveCommand { get; }

        public WindowViewModel()
        {
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow());
            MaximizeCommand = new RelayCommand(_ => ToggleMaximize());
            CloseCommand = new RelayCommand(_ => CloseWindow());
            DragMoveCommand = new RelayCommand(_ => DragMove());
        }

        private void MinimizeWindow()
        {
            WindowState = WindowState.Minimized;
        }

        private void ToggleMaximize()
        {
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseWindow()
        {
            Application.Current.Shutdown();
        }

        private void DragMove()
        {
            // This will be handled by the view with mouse events
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
