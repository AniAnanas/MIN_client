using Client.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class TabListViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TabViewModel> _chats = [];
        private TabViewModel? _selectedChat;
        private string _searchText = string.Empty;

        public TabListViewModel()
        {
            CreateChatCommand = new RelayCommand(_ => CreateNewChat());
            SearchCommand = new RelayCommand(_ => PerformSearch());
            SelectChatCommand = new RelayCommand(chat => SelectChat(chat as TabViewModel));

            CreateMockChats();
        }

        public ObservableCollection<TabViewModel> Chats => _chats;
        public ObservableCollection<TabViewModel> Tabs => Chats;

        public TabViewModel? SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (_selectedChat != value)
                {
                    _selectedChat = value;
                    OnPropertyChanged(nameof(SelectedChat));
                }
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (_searchText != value)
                {
                    _searchText = value;
                    OnPropertyChanged(nameof(SelectedChat));
                    PerformSearch();
                }
            }
        }

        public ICommand CreateChatCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SelectChatCommand { get; }

        // При поступлении новых ChatModel:
        public void AddChat(ChatModel chat)
        {
            var tab = new TabViewModel(chat);
            Tabs.Add(tab);
        }

        // При удалении:
        public void RemoveChat(TabViewModel tab)
        {
            tab.Dispose();
            Tabs.Remove(tab);
        }
        private void SelectChat(TabViewModel? chat)
        {
            if (chat != null)
            {
                SelectedChat = chat;
                Log.Info($"Selected chat: {chat.Title}");
            }
        }
        public void LoadChats()
        {
            // TODO: Load chats from database/cache
            CreateMockChats();
        }
        private void CreateNewChat()
        {
            var rnd = new Random();
            var user = new UserModel(rnd.Next(), "newUser " + _chats.Count);
            var newChat = new ChatModel(
                rnd.Next(),
                user,
                [ new(0, "Первое сообщ", DateTime.Now, user), new(1, "второе сообщ", DateTime.Now + new TimeSpan(rnd.Next(0, 8), 0, 0), new(1, "chupep")) ]
            );

            var chatViewModel = new TabViewModel(newChat);
            _chats.Add(chatViewModel);

            SelectedChat = chatViewModel;
            Log.Info($"Created new chat: {newChat.User.Fullname}");
        }
        private void CreateMockChats()
        {
            var random = new Random();

            for (int i = 0; i < 5; i++)
            {
                var user = new UserModel(i, "user" + i, "User " + (i + 1)) { IsOnline = i % 2 == 0 };
                DateTime timestamp = DateTime.Now.AddMinutes(-random.Next(0, 60));
                var chat = new ChatModel(i, user,
                    [
                        new(0, $"This is first message in Chat {i + 1}", timestamp, user),
                        new(1, $"This is the last message in Chat {i + 1}", timestamp, user),
                    ]
                );

                var chatViewModel = new TabViewModel(chat);
                _chats.Add(chatViewModel);
            }

            Log.Info($"Loaded {Chats.Count} mock chats");
        }
        private void PerformSearch()
        {
            // TODO: Implement search logic
            Log.Info($"Searching for: {SearchText}");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
