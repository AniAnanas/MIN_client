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
        public ObservableCollection<Tab> ChatTabs { get; } = new();
        public ObservableCollection<string> OpenChats { get; } = new();

        private Tab? _selectedChat;
        public Tab? SelectedChat
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
                ChatTabs.Add(new Tab
                {
                    Title = "Чюпеп " + s,
                    LastMessage = "Последнее сообщение " + s,
                    isOnline = i%2 == 0,
                    //LastOnlineTime = (Timestamp.Now + new TimeSpan(rnd.Next(24, 48), 0, 0)).ToString("t"),

                    //Messages =
                    //{
                    //    new Message { SenderName = "Чюпеп " + s, Text = "прив чдкд " + s, Timestamp = DateTime.Now + new TimeSpan(rnd.Next(24), 0, 0) },
                    //    new Message { SenderName = "Я", Text = "прив", Timestamp = DateTime.Now + new TimeSpan(rnd.Next(24, 48), 0, 0) }
                    //}
                });
            }

            SelectedChat = null!;
            if (ChatTabs.Count > 0) {
                SelectedChat = ChatTabs[rnd.Next(0, ChatTabs.Count)];
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
            //SelectedChat.Messages.Add(new Message { SenderName = "Me", Text = Draft });
            SelectedChat.LastMessage = Draft;
            Draft = string.Empty;
            OnPropertyChanged(nameof(SelectedChat));
        }
        private void CreateChat()
        {
            var c = new Tab { Title = "New Chat", LastMessage = "", isOnline = false };
            ChatTabs.Add(c);
            SelectedChat = c;
        }



        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
