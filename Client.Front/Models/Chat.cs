using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Front.Models
{
    public enum ChatType
    {
        Personal,
        Group,
        Channel
    }
    public class ChatModel : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private string? _avatar;
        private string _lastMessage = string.Empty;
        private string _time = string.Empty;
        private int _unreadCount;
        private bool _isOnline;
        private ChatType _type;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Title
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Title)); }
        }

        public string? Avatar
        {
            get => _avatar;
            set { _avatar = value; OnPropertyChanged(nameof(Avatar)); }
        }

        public string LastMessage
        {
            get => _lastMessage;
            set { _lastMessage = value; OnPropertyChanged(nameof(LastMessage)); }
        }

        public string Time
        {
            get => _time;
            set { _time = value; OnPropertyChanged(nameof(Time)); }
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set { _unreadCount = value; OnPropertyChanged(nameof(UnreadCount)); }
        }

        public bool isOnline
        {
            get => _isOnline;
            set { _isOnline = value; OnPropertyChanged(nameof(isOnline)); }
        }

        public ChatType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(nameof(Type)); }
        }

        public string TypeBadgeText => Type switch
        {
            ChatType.Channel => "Канал",
            ChatType.Group => "Группа",
            _ => string.Empty
        };
        public TabModel GetTabFromChat()
        {
            return new TabModel
            {
                Id = Id,
                Title = Title,
                LastMessage = LastMessage,
                isOnline = isOnline,
                Avatar = Avatar,
                UnreadCount = UnreadCount
            };
        }

        public bool ShowTypeBadge => Type != ChatType.Personal;

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
