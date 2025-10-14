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

        public int Id => _message.Id;
        public string Text => _message.Text;
        public DateTime Timestamp => _message.Timestamp;
        public bool IsOwn => _message.IsOwn;
        public UserModel Sender => _message.Sender;
        public string? Avatar => _message.Avatar;
        public string DisplayTime => Timestamp.ToString("HH:mm");

        public string SenderName => IsOwn
            ? "You"
            : Sender.Fullname;

        public bool ShowAvatar => !IsOwn;

        public bool ShowTime => true; // Can be customized based on grouping logic

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
