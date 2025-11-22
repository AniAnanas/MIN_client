using System;
using System.ComponentModel;

namespace Client.Models
{
    public class UserModel : BaseModel
    {
        private long _id = default;
        private string _name = "null";
        private string _lastName = string.Empty;
        private string _username = string.Empty;
        private string? _avatar;
        private bool _isOnline = false;
        public bool itsMeTrustBro = false;
        public long Id
        {
            get => _id;
            set { OnPropertyChanged(ref _id, value, nameof(Id)); }
        }
        public string Name
        {
            get => _name;
            set { OnPropertyChanged(ref _name, value, nameof(Name)); }
        }
        public string LastName
        {
            get => _lastName;
            set { if (_lastName == value) return; _lastName = value; OnPropertyChanged(ref _lastName, value, nameof(LastName)); }
        }
        public string Fullname =>
            (string.IsNullOrWhiteSpace(LastName)
                ? Name
                : string.Concat(Name, " ", LastName)).Truncate(64);
        public string Username
        {
            get => _username;
            set { if (_username == value) return; _username = value; OnPropertyChanged(ref _username, value, nameof(Username)); }
        }
        public string? Avatar
        {
            get => _avatar;
            set { if (_avatar == value) return; _avatar = value; OnPropertyChanged(ref _avatar, value, nameof(Avatar)); }
        }
        public bool IsOnline
        {
            get => _isOnline;
            set { OnPropertyChanged(ref _isOnline, value, nameof(IsOnline)); }
        }

        public UserModel(long id, string username, string name = "Null", string lastName = "", string? avatar = "")
        {
            Id = id;
            Username = username;
            Name = name;
            LastName = lastName;
            Avatar = avatar;
        }
    }
}
