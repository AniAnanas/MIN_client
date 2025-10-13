using Client.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Client.ViewModels
{
    public class TabViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ChatModel _chat;
        private bool _disposed;

        public TabViewModel(ChatModel chat)
        {
            _chat = chat ?? throw new ArgumentNullException(nameof(chat));
            // Initial projection
            Id = _chat.Id;
            Title = _chat.Title;
            Avatar = _chat.Avatar;
            LastMessage = _chat.LastMessage;
            IsOnline = _chat.IsOnline;
            UnreadCount = _chat.UnreadCount;
            // Subscribe to changes
            _chat.PropertyChanged += ChatOnPropertyChanged;
        }

        public string Id { get; private set; } = string.Empty;

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set { if (_title == value) return; _title = value; RaisePropertyChanged(); }
        }

        private string? _avatar;
        public string? Avatar
        {
            get => _avatar;
            set { if (_avatar == value) return; _avatar = value; RaisePropertyChanged(); }
        }

        private MessageModel _lastMessage = new();
        public MessageModel LastMessage
        {
            get => _lastMessage;
            set { if (_lastMessage == value) return; _lastMessage = value; RaisePropertyChanged(); }
        }

        private bool _isOnline;
        public bool IsOnline
        {
            get => _isOnline;
            set { if (_isOnline == value) return; _isOnline = value; RaisePropertyChanged(); }
        }

        private int _unreadCount;
        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                if (_unreadCount == value) return;
                _unreadCount = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasUnread));
            }
        }

        public bool HasUnread => UnreadCount > 0;

        public string TypeBadgeText => _chat.TypeBadgeText;
        public bool ShowTypeBadge => _chat.ShowTypeBadge;

        private void ChatOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Map only properties required by tab
            switch (e.PropertyName)
            {
                case nameof(ChatModel.Id):
                    Id = _chat.Id;
                    break;
                case nameof(ChatModel.Title):
                    Title = _chat.Title;
                    break;
                case nameof(ChatModel.Avatar):
                    Avatar = _chat.Avatar;
                    break;
                case nameof(ChatModel.LastMessage):
                    LastMessage = _chat.LastMessage;
                    break;
                case nameof(ChatModel.IsOnline):
                    IsOnline = _chat.IsOnline;
                    break;
                case nameof(ChatModel.UnreadCount):
                    UnreadCount = _chat.UnreadCount;
                    break;
                case nameof(ChatModel.Type):
                    RaisePropertyChanged(nameof(TypeBadgeText));
                    RaisePropertyChanged(nameof(ShowTypeBadge));
                    break;

                    // other mapped properties...
            }
        }
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        public void Dispose()
        {
            if (_disposed) return;
            _chat.PropertyChanged -= ChatOnPropertyChanged;
            _disposed = true;
        }
    }

}
