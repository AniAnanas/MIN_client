using Client.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Client.ViewModels
{
    public class MessageViewModel : INotifyPropertyChanged
    {
        private readonly MessageModel _message;

        public MessageViewModel(MessageModel message)
        {
            _message = message ?? throw new ArgumentNullException(nameof(message));
            _message.PropertyChanged += Message_PropertyChanged;
        }

        public long Id => _message.Id;
        public string Text => _message.Text;
        public UserModel User => _message.User;
        public bool IsOwn => _message.IsOwn;
        public string? Avatar => _message.User.Avatar;
        public DateTime Timestamp => _message.Timestamp;
        public string DisplayTime => _message.Time;

        public string SenderName => IsOwn
            ? "You"
            : User.Fullname;

        public bool ShowAvatar => !IsOwn;

        public static bool ShowTime => true; // Can be customized based on grouping logic

        private void Message_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(e.PropertyName);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void Dispose()
        {
            _message.PropertyChanged -= Message_PropertyChanged;
        }
    }
}
