using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
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
        //private UserModel[] _members = new UserModel[] { };
        private MessageModel _lastMessage = new();
        private int _unreadCount;
        private bool _isOnline;
        private ChatType _type;

        public string Id
        {
            get => _id;
            set { if (_id == value) return; _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Title
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Title)); }
        }

        public string? Avatar
        {
            //get => Type switch
            //{
            //    ChatType.Personal => _members[0].Avatar,
            //    _ => _avatar
            //};
            get => _avatar;
            set { if (_avatar == value) return; _avatar = value; OnPropertyChanged(nameof(Avatar)); }
        }

        public MessageModel LastMessage
        {
            get => _lastMessage;
            set { if (_lastMessage == value) return; _lastMessage = value; OnPropertyChanged(nameof(LastMessage)); }
        }

        public string Time
        {
            get => (DateTime.Now - LastMessage.Timestamp) < new TimeSpan(1, 0, 0, 0)
                ? LastMessage.Timestamp.ToString("HH:mm")
                : (DateTime.Now - LastMessage.Timestamp) < new TimeSpan(7, 0, 0, 0)
                    ? LastMessage.Timestamp.ToString("ddd")
                    : LastMessage.Timestamp.ToString("dd.MM.yyyy");
        }

        public int UnreadCount
        {
            get => _unreadCount;
            set { if (_unreadCount == value) return; _unreadCount = value; OnPropertyChanged(nameof(UnreadCount)); }
        }

        public bool IsOnline
        {
            get => _isOnline;
            set { if (_isOnline == value) return; _isOnline = value; OnPropertyChanged(nameof(IsOnline)); }
        }

        public ChatType Type
        {
            get => _type;
            set { if (_type == value) return; _type = value; OnPropertyChanged(nameof(Type)); }
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
                isOnline = IsOnline,
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
