using Google.Protobuf.WellKnownTypes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Models
{
    public class ChatModel : BaseModel
    {
        private long _id = default;
        private UserModel _user = new(0, "None", "None");
        private ObservableCollection<MessageModel> _messages;
        private int _unreadCount;
        //private ChatType _type;

        public long Id
        {
            get => _id;
            set { OnPropertyChanged(ref _id, value, nameof(Id)); }
        }
        public UserModel User
        {
            get => _user;
            set { OnPropertyChanged(ref _user, value, nameof(User)); }
        }
        public ObservableCollection<MessageModel> Messages
        {
            get => _messages;
            set { _messages = value; }
        }

        public MessageModel LastMessage
        {
            get => _messages.OrderBy((m) => m.Id).Last();
        }

        public string Time => LastMessage.Time;
        
        public int UnreadCount
        {
            get => _unreadCount;
            set { OnPropertyChanged(ref _unreadCount, value, nameof(UnreadCount)); }
        }
        public bool HasUnread { get => UnreadCount > 0; }

        public ChatModel(int id, UserModel user, ObservableCollection<MessageModel>? messages = null)
        {
            Id = id;
            User = user;
            Messages = messages ?? [];
        }
    }
}
