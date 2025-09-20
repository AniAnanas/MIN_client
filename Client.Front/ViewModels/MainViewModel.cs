using Client.Front.Controls;
using Client.Front.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Client.Front.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public TopBarUI TopBar { get; set; }
        public ObservableCollection<TabModel> ChatTabs { get; } = new();
        public ObservableCollection<string> OpenChats { get; } = new();

        private TabModel? _selectedChat;
        public TabModel? SelectedChat
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

        public MainViewModel()
        {
            //---Sample chats start---
            var rnd = new Random();
            for (int i = 0; i <= 8; i++)
            {
                string s = i.ToString();
                ChatTabs.Add(new TabModel
                {
                    Title = "Чюпеп " + s,
                    LastMessage = "Последнее сообщение " + s,
                    isOnline = i%2 == 0,
                    //LastOnlineTime = (Timestamp.Now + new TimeSpan(rnd.Next(24, 48), 0, 0)).ToString("t"),

                    //Messages =
                    //{
                    //    new MessageModel { SenderName = "Чюпеп " + s, Text = "прив чдкд " + s, Timestamp = DateTime.Now + new TimeSpan(rnd.Next(24), 0, 0) },
                    //    new MessageModel { SenderName = "Я", Text = "прив", Timestamp = DateTime.Now + new TimeSpan(rnd.Next(24, 48), 0, 0) }
                    //}
                });
            }

            TopBar = new TopBarUI();


            SelectedChat = null!;
            if (ChatTabs.Count > 0) {
                SelectedChat = ChatTabs[rnd.Next(0, ChatTabs.Count)];
            }
            //---Sample chats end---

            SendCommand = new RelayCommand(_ => SendMessage(), _ => SelectedChat != null && !string.IsNullOrWhiteSpace(Draft));
            NewChatCommand = new RelayCommand(_ => CreateChat());
            SearchCommand = new RelayCommand(_ => { });
            SettingsCommand = new RelayCommand(_ => { });
            MinimizeWindowCommand = new RelayCommand(p => {
                if (p is Window window) window.WindowState = WindowState.Minimized;
            });

            MaximizeWindowCommand = new RelayCommand(p => {
                if (p is Window window)
                {
                    window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
                    if (window.WindowState == WindowState.Maximized)
                    {
                        window.WindowState = WindowState.Normal;
                        if (topBar.FindName("MaximizeBtn") is Button maximizeBtn)
                        {
                            maximizeBtn.Content = "□";
                        }
                    }
                    else
                    {
                        window.WindowState = WindowState.Maximized;
                        if (topBar.FindName("MaximizeBtn") is Button maximizeBtn)
                        {
                            maximizeBtn.Content = "❐";
                        }
                    }
                }
            });

            CloseWindowCommand = new RelayCommand(p => {
                if (p is Window window) window.Close();
            });
        }
        private void SendMessage()
        {
            if (SelectedChat == null)
            {
                Log.Error("SelectedChat == null, called SendMessage");
                return;
            }
            //SelectedChat.Messages.Add(new MessageModel { SenderName = "Me", Text = Draft });
            SelectedChat.LastMessage = Draft;
            Draft = string.Empty;
            OnPropertyChanged(nameof(SelectedChat));
        }
        private void CreateChat()
        {
            var c = new TabModel { Title = "New ChatUI", LastMessage = "", isOnline = false };
            ChatTabs.Add(c);
            SelectedChat = c;
        }
        public ICommand SendCommand { get; }
        public ICommand NewChatCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SettingsCommand { get; }
        public ICommand MinimizeWindowCommand { get; }
        public ICommand MaximizeWindowCommand { get; }
        public ICommand CloseWindowCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;
        void OnPropertyChanged([CallerMemberName] string n = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(n));
    }
}
