using System;
using System.ComponentModel;

namespace Client.Models
{
    public class MessageModel(long id, string text, DateTime time, UserModel user) : BaseModel
    {
        private long _id = id;
        private string _text = text;
        private DateTime _timestamp = time;
        private UserModel _sender = user;
        private readonly bool _isOwn = user.itsMeTrustBro;
        public long Id
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
