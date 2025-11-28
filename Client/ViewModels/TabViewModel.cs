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
            Title = _chat.User.Fullname;
            Avatar = _chat.User.Avatar;
            LastMessage = _chat.LastMessage;
            IsOnline = _chat.User.IsOnline;
            UnreadCount = _chat.UnreadCount;
            // Subscribe to changes
            _chat.PropertyChanged += ChatOnPropertyChanged;
        }

        public long Id { get; private set; } = default;

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set { if (_title == value) return; _title = value; RaisePropertyChanged(nameof(Title)); }
        }

        private string? _avatar;
        public string? Avatar
        {
            get => _avatar;
            set { if (_avatar == value) return; _avatar = value; RaisePropertyChanged(nameof(Avatar)); }
        }

        private MessageModel _lastMessage;
        public MessageModel LastMessage
        {
            get => _lastMessage;
            set { if (_lastMessage == value) return; _lastMessage = value; RaisePropertyChanged(nameof(LastMessage)); }
        }

        private bool _isOnline;
        public bool IsOnline
        {
            get => _isOnline;
            set { if (_isOnline == value) return; _isOnline = value; RaisePropertyChanged(nameof(IsOnline)); }
        }

        private int _unreadCount;
        public int UnreadCount
        {
            get => _unreadCount;
            set
            {
                if (_unreadCount == value) return;
                _unreadCount = value;
                RaisePropertyChanged(nameof(HasUnread));
            }
        }
        public string Time 
        { 
            get 
            {
                if (LastMessage == null)
                    return "";
                TimeSpan span = DateTime.Now - LastMessage.Timestamp;
                return span < new TimeSpan(1, 0, 0, 0)
                    ? LastMessage.Timestamp.ToString("HH:mm")
                    : span < new TimeSpan(7, 0, 0, 0)
                        ? LastMessage.Timestamp.ToString("ddd")
                        : LastMessage.Timestamp.ToString("dd.MM.yyyy");
            } 
        }

        public bool HasUnread => UnreadCount > 0;

        private void ChatOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Map only properties required by tab
            switch (e.PropertyName)
            {
                case nameof(ChatModel.Id):
                    Id = _chat.Id;
                    RaisePropertyChanged(nameof(Id));
                    break;
                case nameof(ChatModel.User):
                    Title = _chat.User.Fullname;
                    Avatar = _chat.User.Avatar;
                    IsOnline = _chat.User.IsOnline;
                    break;
                case nameof(ChatModel.User.Fullname):
                    Title = _chat.User.Fullname;
                    break;
                case nameof(ChatModel.User.Avatar):
                    Avatar = _chat.User.Avatar;
                    break;
                case nameof(ChatModel.LastMessage):
                    LastMessage = _chat.LastMessage;
                    RaisePropertyChanged(nameof(Time));
                    break;
                case nameof(ChatModel.User.IsOnline):
                    IsOnline = _chat.User.IsOnline;
                    break;
                case nameof(ChatModel.UnreadCount):
                    UnreadCount = _chat.UnreadCount;
                    RaisePropertyChanged(nameof(HasUnread));
                    break;
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
