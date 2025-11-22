using Client.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;


namespace Client.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ChatModel _chat;
        private bool _disposed;

        public ChatViewModel(ChatModel chat)
        {
            _chat = chat ?? throw new ArgumentNullException(nameof(chat));
            _chat.PropertyChanged += ChatOnPropertyChanged;
        }

        public long Id => _chat.Id;
        public string Title
        {
            get => _chat.User.Fullname;
            set { if (_chat.User.Name == value) return; _chat.User.Name = value; /* ChatModel уведомит обратно */ }
        }

        public string? Avatar
        {
            get => _chat.User.Avatar;
            set { if (_chat.User.Avatar == value) return; _chat.User.Avatar = value; }
        }

        public int UnreadCount
        {
            get => _chat.UnreadCount;
            set => _chat.UnreadCount = value;
        }

        public ObservableCollection<MessageModel> Messages => _chat.Messages;

        public bool IsOnline => _chat.User.IsOnline;

        // Пример реакции на изменения доменной модели (например, обновить UI)
        private void ChatOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Простая прокси-реакция: уведомляем view о соответствующих свойствах
            switch (e.PropertyName)
            {
                case nameof(ChatModel.User.Fullname):
                    RaisePropertyChanged(nameof(Title));
                    break;
                case nameof(ChatModel.User.Avatar):
                    RaisePropertyChanged(nameof(Avatar));
                    break;
                case nameof(ChatModel.UnreadCount):
                    RaisePropertyChanged(nameof(UnreadCount));
                    break;
                case nameof(ChatModel.User.IsOnline):
                    RaisePropertyChanged(nameof(IsOnline));
                    break;
                    // и т.д.
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _chat.PropertyChanged -= ChatOnPropertyChanged;
            _disposed = true;
        }


        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
