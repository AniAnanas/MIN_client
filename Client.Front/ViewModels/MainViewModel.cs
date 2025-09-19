using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Client.Front.Models;

namespace Client.Front.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ChatTab> Chats { get; } = new();
        public ObservableCollection<string> OpenChats { get; } = new();
        private ChatTab? _selectedChat;
        public ChatTab? SelectedChat
        {
            get { return _selectedChat; }
            set { _selectedChat = value; OnPropertyChanged(); }
        }
        private string _draft = string.Empty;
        public string Draft
        {
            get => _draft;
            set { _draft = value; OnPropertyChanged(); }
        }

        public ICommand SendCommand { get; }
        public ICommand NewChatCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SettingsCommand { get; }

        public MainViewModel()
        {
            //---Sample chats start---
            var rnd = new Random();
            for (int i = 0; i <= 8; i++)
            {
                string s = i.ToString();
                Chats.Add(new ChatTab
                {
                    Title = "Чюпеп " + s,
                    LastPreview = "Последнее сообщение " + s,
                    Status = i%2 == 0 ? "online" : "offline",
                    Messages =
                    {
                        new Message { Sender = "Чюпеп " + s, Content = "прив чдкд " + s, Timestamp = DateTime.Now + new TimeSpan(rnd.Next(24), 0, 0) },
                        new Message { Sender = "Я", Content = "прив", Timestamp = DateTime.Now + new TimeSpan(rnd.Next(24, 48), 0, 0) }
                    }
                });
            }

            SelectedChat = null!;
            if (Chats.Count > 0) {
                SelectedChat = Chats[0];
                OpenChats.Add( Chats[rnd.Next(0, Chats.Count)].Title );
                OpenChats.Add( Chats[rnd.Next(0, Chats.Count)].Title );
            }
            //---Sample chats end---

            SendCommand = new RelayCommand(_ => SendMessage(), _ => SelectedChat != null && !string.IsNullOrWhiteSpace(Draft));
            NewChatCommand = new RelayCommand(_ => CreateChat());
            SearchCommand = new RelayCommand(_ => { });
            SettingsCommand = new RelayCommand(_ => { });
        }
        private void SendMessage()
        {
            if (SelectedChat == null)
            {
                Log.Error("SelectedChat == null, called SendMessage");
                return;
            }
            SelectedChat.Messages.Add(new Message { Sender = "Me", Content = Draft });
            SelectedChat.LastPreview = Draft;
            Draft = string.Empty;
            OnPropertyChanged(nameof(SelectedChat));
        }
        private void CreateChat()
        {
            var c = new ChatTab { Title = "New Chat", LastPreview = "", Status = "offline" };
            Chats.Add(c);
            SelectedChat = c;
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
