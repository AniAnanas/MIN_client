using Client.Data;
using Client.Models;
using Client.Services;
using Client.Shared.Interfaces;
using System.ComponentModel;
using System.Windows;

namespace Client.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        // ViewModels
        public WindowViewModel WindowViewModel { get; }
        public TopBarViewModel TopBarViewModel { get; }
        public TabListViewModel TabListViewModel { get; }

        private ChatViewModel? _currentChatViewModel;
        public ChatViewModel? CurrentChatViewModel
        {
            get => _currentChatViewModel;
            private set
            {
                if (_currentChatViewModel != value)
                {
                    _currentChatViewModel?.Dispose();
                    _currentChatViewModel = value;
                    OnPropertyChanged(nameof(CurrentChatViewModel));
                    OnPropertyChanged(nameof(IsChatSelected));
                }
            }
        }
        public bool IsChatSelected => CurrentChatViewModel != null;

        public MainWindow Window { get; }

        // Services
        private readonly INetworkService _networkService;
        private readonly DatabaseService _databaseService;

        public MainViewModel(MainWindow window)
        {
            Window = window;

            // Initialize services
            _networkService = App.Current.Resources["Net"] as INetworkService
                ?? throw new System.NullReferenceException("NetworkService not found");
            _databaseService = App.Current.Resources["Database"] as DatabaseService
                ?? throw new System.NullReferenceException("DatabaseService not found");

            // Initialize ViewModels
            WindowViewModel = new WindowViewModel();
            TopBarViewModel = new TopBarViewModel(WindowViewModel);
            TabListViewModel = new TabListViewModel();

            SubscribeToNetworkEvents();

            TabListViewModel.PropertyChanged += OnTabListPropertyChanged;

            Log.Info("MainViewModel initialized successfully");
        }

        public async Task InitializeDataAsync(UserModel[] users)
        {
            try
            {
                Log.Info($"MainViewModel: Loading chats for {users.Length} users...");

                var dbChats = _databaseService.GetAllChats();
                Log.Info($"Loaded {dbChats.Count} chats from database");

                // Загружаем чаты из пользователей
                await TabListViewModel.LoadRealChatsAsync(users, dbChats);

                Log.Success($"MainViewModel: {TabListViewModel.Chats.Count} chats loaded");
            }
            catch (Exception ex)
            {
                Log.Error($"InitializeDataAsync failed: {ex.Message}");
            }
        }

        private void SubscribeToNetworkEvents()
        {
            // Подписываемся на входящие сообщения
            _networkService.MessageReceived += OnMessageReceived;

            // Подписываемся на изменения статуса пользователей
            _networkService.UserStatusChanged += OnUserStatusChanged;

            Log.Info("Subscribed to network events");
        }

        private void OnMessageReceived(MessageModel message)
        {
            try
            {
                Log.Info($"New message received from {message.User.Id}: {message.Text}");

                // Сохраняем сообщение в БД
                long chatId = message.User.Id; // ID собеседника
                bool isOutgoing = message.User.Id == _networkService.CurrentUserId;

                _databaseService.SaveMessageFromModel(message, chatId, isOutgoing);

                // Обновляем UI через TabListViewModel
                TabListViewModel.HandleNewMessage(message, chatId);

                // Если этот чат открыт, добавляем сообщение в ChatViewModel
                if (CurrentChatViewModel != null && CurrentChatViewModel.Id == chatId)
                {
                    CurrentChatViewModel.AddIncomingMessage(message);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"OnMessageReceived error: {ex.Message}");
            }
        }

        private void OnUserStatusChanged((long userId, bool isOnline) status)
        {
            Log.Info($"User {status.userId} status changed: {(status.isOnline ? "online" : "offline")}");

            // Обновляем статус в БД
            _databaseService.UpdateUserOnlineStatus(status.userId, status.isOnline);

            // Обновляем UI
            TabListViewModel.UpdateUserStatus(status.userId, status.isOnline);
        }

        private void OnTabListPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TabListViewModel.SelectedChat))
            {
                var selectedChat = TabListViewModel.SelectedChat;
                if (selectedChat != null)
                {
                    Log.Info($"Selected chat changed to: {selectedChat.Title}");

                    // Получаем ChatModel из TabListViewModel
                    var chatModel = TabListViewModel.GetChatModelById(selectedChat.Id);

                    if (chatModel != null)
                    {
                        // Создаем новый ChatViewModel
                        CurrentChatViewModel = new ChatViewModel(chatModel);
                    }
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
