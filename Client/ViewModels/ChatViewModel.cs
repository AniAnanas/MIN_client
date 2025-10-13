using Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            // Если нужны сообщения — сюда можно подключать ObservableCollection<MessageViewModel>
        }

        public string Id => _chat.Id;
        public string Title
        {
            get => _chat.Title;
            set { if (_chat.Title == value) return; _chat.Title = value; /* ChatModel уведомит обратно */ }
        }

        public string? Avatar
        {
            get => _chat.Avatar;
            set { if (_chat.Avatar == value) return; _chat.Avatar = value; }
        }

        public int UnreadCount
        {
            get => _chat.UnreadCount;
            set => _chat.UnreadCount = value;
        }

        public bool IsOnline => _chat.IsOnline;

        // Пример реакции на изменения доменной модели (например, обновить UI)
        private void ChatOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Простая прокси-реакция: уведомляем view о соответствующих свойствах
            switch (e.PropertyName)
            {
                case nameof(ChatModel.Title):
                    RaisePropertyChanged(nameof(Title));
                    break;
                case nameof(ChatModel.Avatar):
                    RaisePropertyChanged(nameof(Avatar));
                    break;
                case nameof(ChatModel.UnreadCount):
                    RaisePropertyChanged(nameof(UnreadCount));
                    break;
                case nameof(ChatModel.IsOnline):
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
