using System;
using System.ComponentModel;

namespace Client.Models
{
    public class MessageModel : BaseModel
    {
        public MessageModel(int id, string text, DateTime time, UserModel user) 
        {
            _id = id;
            _text = text;
            _timestamp = time;
            _sender = user;
            _isOwn = user.itsMeTrustBro;
        }
        
        private int _id = default;
        private string _text = string.Empty;
        private DateTime _timestamp = DateTime.UnixEpoch;
        private UserModel _sender = new(0, "null");
        private bool _isOwn;
        public int Id
        {
            get => _id;
            set { OnPropertyChanged(ref _id, value, nameof(Id)); }
        }
        public bool IsOwn
        {
            get => _isOwn;
        }
        public UserModel User
        {
            get => _sender;
            set { OnPropertyChanged(ref _sender, value, nameof(User)); }
        }
        public string Text
        {
            get => _text;
            set { OnPropertyChanged(ref _text, value, nameof(Text)); }
        }
        public DateTime Timestamp
        {
            get => _timestamp;
            set { OnPropertyChanged(ref _timestamp, value, nameof(Timestamp)); }
        }

        public string Time
        {
            get
            {
                TimeSpan span = DateTime.Now - _timestamp;
                return span < new TimeSpan(1, 0, 0, 0)
                ? _timestamp.ToString("HH:mm")
                : span < new TimeSpan(7, 0, 0, 0)
                    ? _timestamp.ToString("ddd")
                    : _timestamp.ToString("dd.MM.yyyy");
            }
        }

        public override bool Equals(object? obj)
        {
            return obj is MessageModel model &&
                   _id == model._id &&
                   _text == model._text &&
                   _timestamp == model._timestamp &&
                   EqualityComparer<UserModel>.Default.Equals(_sender, model._sender) &&
                   _isOwn == model._isOwn;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(_id, _text, _timestamp, _sender, _isOwn);
        }
    }
}
