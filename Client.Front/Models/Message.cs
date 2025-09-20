using System;
using System.ComponentModel;

namespace Client.Front.Models
{
    public class Message : INotifyPropertyChanged
    {
        private int _id = default;
        private string _text = string.Empty;
        private DateTime _timestamp = DateTime.UnixEpoch;
        private bool _isOwn;
        private string _senderName = "null";
        private string? _avatar;
        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }
        public bool IsOwn
        {
            get => _isOwn;
            set { _isOwn = value; OnPropertyChanged(nameof(IsOwn)); }
        }
        public string SenderName
        {
            get => _senderName;
            set { _senderName = value; OnPropertyChanged(nameof(SenderName)); }
        }
        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }
        public DateTime Timestamp
        {
            get => _timestamp;
            set { _timestamp = value; OnPropertyChanged(nameof(Timestamp)); }
        }
        public string? Avatar
        {
            get => _avatar;
            set { _avatar = value; OnPropertyChanged(nameof(Avatar)); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
