using Client.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            var newChat = new ChatModel
            {
                Id = Guid.NewGuid().ToString(),
                Title = "New Chat",
                IsOnline = false,
                Type = ChatType.Personal,
                LastMessage = new MessageModel
                {
                    Sender = new UserModel(new Random().Next(), "newUser " + _chats.Count),
                }
            };

            var chatViewModel = new TabViewModel(newChat);
            _chats.Add(chatViewModel);

            SelectedChat = chatViewModel;
            Log.Info($"Created new chat: {newChat.Title}");
        }
        private void CreateMockChats()
        {
            var random = new Random();

            for (int i = 0; i < 5; i++)
            {
                DateTime timestamp = DateTime.Now.AddMinutes(-random.Next(0, 60));
                var chat = new ChatModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"Chat {i + 1}",
                    Avatar = null,
                    LastMessage = new MessageModel
                    {
                        Id = random.Next(),
                        Sender = new UserModel(i, "user" + i),
                        Text = $"This is the last message in Chat {i + 1}",
                        Timestamp = timestamp
                    },
                    IsOnline = i % 2 == 0,
                    Type = i % 3 == 0 ? ChatType.Group : ChatType.Personal,
                    UnreadCount = random.Next(0, 10),
                };

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
