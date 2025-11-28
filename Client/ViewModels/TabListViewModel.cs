using Client.Data;
using Client.Models;
using Client.Shared.Interfaces;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using System.Windows.Input;

namespace Client.ViewModels
{
    public class TabListViewModel : INotifyPropertyChanged
    {
        private readonly INetworkService _networkService;
        private readonly DatabaseService _databaseService;
        private readonly long _currentUserId;

        private ObservableCollection<TabViewModel> _chats = [];
        private TabViewModel? _selectedChat;

        // Словарь для быстрого доступа к ChatModel по userId
        private Dictionary<long, ChatModel> _chatModels = new();

        public TabListViewModel()
        {
            _networkService = App.Current.Resources["Net"] as INetworkService
                ?? throw new NullReferenceException("NetworkService not found");
            _databaseService = App.Current.Resources["Database"] as DatabaseService
                ?? throw new NullReferenceException("DatabaseService not found");
            _currentUserId = _databaseService.GetCurrentUserId();

            CreateChatCommand = new RelayCommand(_ => CreateNewChat());
            SelectChatCommand = new RelayCommand(chat => SelectChat(chat as TabViewModel));
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

        public ICommand CreateChatCommand { get; }
        public ICommand SelectChatCommand { get; }


        /// <summary>
        /// Получает ChatModel по ID для создания ChatViewModel
        /// </summary>
        public ChatModel? GetChatModelById(long chatId)
        {
            return _chatModels.TryGetValue(chatId, out var chatModel) ? chatModel : null;
        }

        /// <summary>
        /// Загружает реальные чаты из пользователей и БД
        /// </summary>
        public async Task LoadRealChatsAsync(
            UserModel[] allUsers,
            List<(long chatId, UserModel user, string lastMessage, DateTime lastMessageTime, int unreadCount)> dbChats)
        {
            try
            {
                Log.Info($"MainViewModel: Loading chats for {allUsers.Length} users...");

                _chats.Clear();
                _chatModels.Clear();

                // Создаем словарь для быстрого поиска пользователей
                var usersDict = allUsers.ToDictionary(u => u.Id);

                // 1. Загружаем чаты с историей из БД
                foreach (var dbChat in dbChats)
                {
                    if (usersDict.TryGetValue(dbChat.chatId, out var user))
                    {
                        // Загружаем последние сообщения из БД
                        var messages = _databaseService.GetChatHistory(dbChat.chatId, limit: 50);

                        var chatModel = new ChatModel(
                            (int)dbChat.chatId,
                            user,
                            new ObservableCollection<MessageModel>(messages)
                        )
                        {
                            UnreadCount = dbChat.unreadCount
                        };

                        _chatModels[dbChat.chatId] = chatModel;

                        var tabViewModel = new TabViewModel(chatModel);
                        _chats.Add(tabViewModel);

                        // Помечаем, что этот пользователь уже добавлен
                        usersDict.Remove(dbChat.chatId);
                    }
                }

                // 2. Добавляем остальных пользователей без истории (потенциальные собеседники)
                foreach (var user in usersDict.Values)
                {
                    // Пропускаем самого себя
                    if (user.Id == _currentUserId)
                        continue;

                    // Создаем пустой чат (без сообщений) с placeholder сообщением
                    var placeholderMessage = new MessageModel(
                        -1,
                        "Начните диалог...",
                        DateTime.Now,
                        user
                    );

                    // Создаем пустой чат (без сообщений)
                    var chatModel = new ChatModel(
                        (int)user.Id,
                        user,
                        new ObservableCollection<MessageModel> { placeholderMessage }
                    );

                    _chatModels[user.Id] = chatModel;

                    var tabViewModel = new TabViewModel(chatModel);
                    _chats.Add(tabViewModel);
                }

                // Сортируем: чаты с реальными сообщениями сверху
                var sortedChats = _chats
                    .OrderByDescending(c => {
                        // Если есть реальное сообщение (Id != -1), используем его время
                        if (c.LastMessage != null && c.LastMessage.Id != -1)
                            return c.LastMessage!.Timestamp;
                        // Иначе отправляем в конец
                        return DateTime.MinValue;
                    })
                    .ToList();

                _chats.Clear();
                foreach (var chat in sortedChats)
                {
                    _chats.Add(chat);
                }

                var newChats = usersDict.Count - (usersDict.ContainsKey(_currentUserId) ? 1 : 0);
                Log.Success($"Loaded {_chats.Count} real chats ({dbChats.Count} with history, {newChats} new)");
            }
            catch (Exception ex)
            {
                Log.Error($"LoadRealChatsAsync failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Обрабатывает новое входящее сообщение
        /// </summary>
        public void HandleNewMessage(MessageModel message, long chatId)
        {
            try
            {
                if (_chatModels.TryGetValue(chatId, out var chatModel))
                {
                    App.Current.Dispatcher.Invoke(() =>
                    {
                        // Добавляем сообщение в существующий чат
                        //chatModel.Messages.Add(message);
                        chatModel.UnreadCount++;
                        // Перемещаем чат наверх списка
                        var tab = _chats.FirstOrDefault(t => t.Id == chatId);
                        if (tab != null)
                        {
                            _chats.Remove(tab);
                            _chats.Insert(0, tab);
                        }
                    });
                }
                else
                {
                    // Создаем новый чат для нового собеседника
                    Log.Warn($"Received message from unknown user {chatId}, creating new chat");

                    var user = _databaseService.GetUserById(chatId);
                    if (user != null)
                    {
                        var newChatModel = new ChatModel(
                            (int)chatId,
                            user,
                            new ObservableCollection<MessageModel> { message }
                        )
                        {
                            UnreadCount = 1
                        };

                        _chatModels[chatId] = newChatModel;

                        var tabViewModel = new TabViewModel(newChatModel);
                        _chats.Insert(0, tabViewModel);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"HandleNewMessage failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Обновляет статус пользователя
        /// </summary>
        public void UpdateUserStatus(long userId, bool isOnline)
        {
            try
            {
                if (_chatModels.TryGetValue(userId, out var chatModel))
                {
                    chatModel.User.IsOnline = isOnline;
                    Log.Info($"Updated user {userId} status to {(isOnline ? "online" : "offline")}");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"UpdateUserStatus failed: {ex.Message}");
            }
        }

        // При поступлении новых ChatModel:
        public void AddChat(ChatModel chat)
        {
            var tab = new TabViewModel(chat);
            Tabs.Add(tab);
            _chatModels[chat.User.Id] = chat;
        }

        // При удалении:
        public void RemoveChat(TabViewModel tab)
        {
            tab.Dispose();
            Tabs.Remove(tab);
            _chatModels.Remove(tab.Id);
        }

        private void SelectChat(TabViewModel? chat)
        {
            if (chat != null)
            {
                SelectedChat = chat;

                // Помечаем чат как прочитанный
                if (chat.UnreadCount > 0)
                {
                    _databaseService.MarkChatAsRead(chat.Id);

                    if (_chatModels.TryGetValue(chat.Id, out var chatModel))
                    {
                        chatModel.UnreadCount = 0;
                    }
                }

                Log.Info($"Selected chat: {chat.Title}");
            }
        }

        private void CreateNewChat()
        {
            // TODO: Открыть диалог поиска пользователей
            Log.Info("CreateNewChat clicked - open user search dialog");
        }

        //private void CreateNewChat()
        //{
        //    var rnd = new Random();
        //    var user = new UserModel(rnd.Next(), "newUser " + _chats.Count);
        //    var newChat = new ChatModel(
        //        rnd.Next(),
        //        user,
        //        [ new(0, "Первое сообщ", DateTime.Now, user), new(1, "второе сообщ", DateTime.Now + new TimeSpan(rnd.Next(0, 8), 0, 0), new(1, "chupep")) ]
        //    );

        //    var chatViewModel = new TabViewModel(newChat);
        //    _chats.Add(chatViewModel);

        //    SelectedChat = chatViewModel;
        //    Log.Info($"Created new chat: {newChat.User.Fullname}");
        //}
        //private void CreateMockChats()
        //{
        //    var random = new Random();

        //    for (int i = 0; i < 5; i++)
        //    {
        //        var user = new UserModel(i, "user" + i, "User " + (i + 1)) { IsOnline = i % 2 == 0 };
        //        DateTime timestamp = DateTime.Now.AddMinutes(-random.Next(0, 60));
        //        var chat = new ChatModel(i, user,
        //            [
        //                new(0, $"This is first message in Chat {i + 1}", timestamp, user),
        //                new(1, $"This is the last message in Chat {i + 1}", timestamp, user),
        //            ]
        //        );

        //        var chatViewModel = new TabViewModel(chat);
        //        _chats.Add(chatViewModel);
        //    }

        //    Log.Info($"Loaded {Chats.Count} mock chats");
        //}

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
