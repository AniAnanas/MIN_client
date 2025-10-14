using System;
using System.ComponentModel;

namespace Client.Models
{
    public class UserModel : INotifyPropertyChanged
    {
        private int _id = default;
        private string _name = "null";
        private string _lastName = string.Empty;
        private string _username = string.Empty;
        private string? _avatar;
        public int Id
        {
            get => _id;
            set { if (_id == value) return; _id = value; OnPropertyChanged(nameof(Id)); }
        }
        public string Name
        {
            get => _name;
            set { if (_name == value) return; _name = value; OnPropertyChanged(nameof(Name)); }
        }
        public string LastName
        {
            get => _lastName;
            set { if (_lastName == value) return; _lastName = value; OnPropertyChanged(nameof(LastName)); }
        }
        public string Fullname =>
            (string.IsNullOrWhiteSpace(LastName)
                ? Name
                : string.Concat(Name, " ", LastName)).Truncate(64);
        public string Username
        {
            get => _username;
            set { if (_username == value) return; _username = value; OnPropertyChanged(nameof(Username)); }
        }
        public string? Avatar
        {
            get => _avatar;
            set { if (_avatar == value) return; _avatar = value; OnPropertyChanged(nameof(Avatar)); }
        }

        public UserModel(int id, string username, string name = "", string lastName = "", string? avatar = "")
        {
            Id = id;
            Username = username;
            Name = name;
            LastName = lastName;
            Avatar = avatar;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
