using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class TopBarViewModel : INotifyPropertyChanged
    {
        private readonly WindowViewModel _windowViewModel;

        public TopBarViewModel(WindowViewModel windowViewModel)
        {
            _windowViewModel = windowViewModel ?? throw new ArgumentNullException(nameof(windowViewModel));
        }

        public string WindowTitle => _windowViewModel.Title;
        public WindowState CurrentWindowState => _windowViewModel.WindowState;

        public ICommand MinimizeCommand => _windowViewModel.MinimizeCommand;
        public ICommand MaximizeCommand => _windowViewModel.MaximizeCommand;
        public ICommand CloseCommand => _windowViewModel.CloseCommand;


        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
