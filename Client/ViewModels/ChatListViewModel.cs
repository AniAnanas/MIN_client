using Client.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class ChatListViewModel : INotifyPropertyChanged
    {
        private readonly ObservableCollection<ChatViewModel> _chats = new();
        private ChatViewModel? _selectedChat;
        private string _searchText = string.Empty;

        public ChatListViewModel()
        {
            CreateChatCommand = new RelayCommand(_ => CreateNewChat());
            SearchCommand = new RelayCommand(_ => PerformSearch());
            SelectChatCommand = new RelayCommand(chat => SelectChat(chat as ChatViewModel));
        }

        public ObservableCollection<ChatViewModel> Chats => _chats;

        public ChatViewModel? SelectedChat
        {
            get => _selectedChat;
            set
            {
                if (_selectedChat != value)
                {
                    _selectedChat = value;
                    OnPropertyChanged();
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
                    OnPropertyChanged();
                    PerformSearch();
                }
            }
        }

        public ICommand CreateChatCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SelectChatCommand { get; }

        private void CreateNewChat()
        {
            var newChat = new ChatModel
            {
                Id = Guid.NewGuid().ToString(),
                Title = "New Chat",
                IsOnline = false,
                Type = ChatType.Personal
            };

            var chatViewModel = new ChatViewModel(newChat);
            _chats.Add(chatViewModel);

            SelectedChat = chatViewModel;
            Log.Info($"Created new chat: {newChat.Title}");
        }

        private void PerformSearch()
        {
            // TODO: Implement search logic
            Log.Info($"Searching for: {SearchText}");
        }

        private void SelectChat(ChatViewModel? chat)
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

        private void CreateMockChats()
        {
            var random = new Random();

            for (int i = 0; i < 5; i++)
            {
                var chat = new ChatModel
                {
                    Id = Guid.NewGuid().ToString(),
                    Title = $"Chat {i + 1}",
                    IsOnline = i % 2 == 0,
                    Type = i % 3 == 0 ? ChatType.Group : ChatType.Personal,
                    UnreadCount = random.Next(0, 10)
                };

                var chatViewModel = new ChatViewModel(chat);
                _chats.Add(chatViewModel);
            }

            Log.Info($"Loaded {Chats.Count} mock chats");
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
