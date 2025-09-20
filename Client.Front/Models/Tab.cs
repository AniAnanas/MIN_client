using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;

namespace Client.Front.Models
{
    public class TabModel : INotifyPropertyChanged
    {
        private string _id = string.Empty;
        private string _title = string.Empty;
        private string? _avatar;
        private int _unreadCount = 0;
        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }
        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }
        public string? Avatar
        {
            get => _avatar;
            set { _avatar = value; OnPropertyChanged(nameof(Avatar)); }
        }
        public string LastMessage { get; set; } = string.Empty;
        public bool isOnline { get; set; } = false;
        public int UnreadCount 
        { 
            get => _unreadCount; 
            set { _unreadCount = value; OnPropertyChanged(nameof(UnreadCount)); }
        }
        public bool HasUnread { get => UnreadCount > 0; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
