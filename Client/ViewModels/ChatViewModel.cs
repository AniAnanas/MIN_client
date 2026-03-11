using Client.Data;
using Client.Models;
using Client.Shared.Interfaces;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;


namespace Client.ViewModels
{
    public class ChatViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly ChatModel _chat;
        private readonly INetworkService _networkService;
        private readonly DatabaseService _databaseService;
        private readonly long _currentUserId;
        private bool _disposed;

        private string _messageText = string.Empty;
        private bool _isSending = false;
        public ChatViewModel(ChatModel chat)
        {
            _chat = chat ?? throw new ArgumentNullException(nameof(chat));

            _networkService = App.Current.Resources["Net"] as INetworkService
                ?? throw new NullReferenceException("App.Current.Resources[\"Net\"]");
            _databaseService = App.Current.Resources["Database"] as DatabaseService
                ?? throw new NullReferenceException("App.Current.Resources[\"Database\"]");
            _currentUserId = _networkService.CurrentUserId;

            _chat.PropertyChanged += ChatOnPropertyChanged;

            // Commands
            SendMessageCommand = new RelayCommand(
                async _ => await SendMessageAsync(),
                _ => CanSendMessage()
            );

            LoadMoreMessagesCommand = new RelayCommand(
                async _ => await LoadMoreMessagesAsync()
            );

            // Загружаем историю при создании
            _ = LoadHistoryAsync();
        }

        #region Properties

        public long Id => _chat.Id;
        public string Title
        {
            get => _chat.User.Fullname;
            set { if (_chat.User.Name == value) return; _chat.User.Name = value; /* ChatModel уведомит обратно */ }
        }

        public string? Avatar
        {
            get => _chat.User.Avatar;
            set { if (_chat.User.Avatar == value) return; _chat.User.Avatar = value; }
        }

        public int UnreadCount
        {
            get => _chat.UnreadCount;
            set => _chat.UnreadCount = value;
        }

        public ObservableCollection<MessageModel> Messages => _chat.Messages;

        public bool IsOnline => _chat.User.IsOnline;

        public string MessageText
        {
            get => _messageText;
            set
            {
                if (_messageText != value)
                {
                    _messageText = value;
                    RaisePropertyChanged(nameof(MessageText));
                    ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
                }
            }
        }

        public bool IsSending
        {
            get => _isSending;
            set
            {
                if (_isSending != value)
                {
                    _isSending = value;
                    RaisePropertyChanged(nameof(IsSending));
                    ((RelayCommand)SendMessageCommand).RaiseCanExecuteChanged();
                }
            }
        }

        #endregion

        #region Commands

        public ICommand SendMessageCommand { get; }
        public ICommand LoadMoreMessagesCommand { get; }

        #endregion

        #region Methods

        /// <summary>
        /// Загружает историю сообщений из БД
        /// </summary>
        private async Task LoadHistoryAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    var messages = _databaseService.GetChatHistory(_chat.User.Id, limit: 50);

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        _chat.Messages.Clear();
                        foreach (var msg in messages)
                        {
                            _chat.Messages.Add(msg);
                        }
                    });

                    Log.Info($"Loaded {messages.Count} messages for chat {_chat.User.Id}");
                });
            }
            catch (Exception ex)
            {
                Log.Error($"LoadHistoryAsync failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Загружает более старые сообщения
        /// </summary>
        private async Task LoadMoreMessagesAsync()
        {
            try
            {
                if (_chat.Messages.Count == 0)
                    return;

                var oldestMessageId = _chat.Messages.Min(m => m.Id);

                await Task.Run(() =>
                {
                    var messages = _databaseService.GetChatHistory(
                        _chat.User.Id,
                        limit: 50,
                        beforeMessageId: oldestMessageId
                    );

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        foreach (var msg in messages)
                        {
                            _chat.Messages.Insert(0, msg);
                        }
                    });

                    Log.Info($"Loaded {messages.Count} more messages for chat {_chat.User.Id}");
                });
            }
            catch (Exception ex)
            {
                Log.Error($"LoadMoreMessagesAsync failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Отправляет сообщение
        /// </summary>
        private async Task SendMessageAsync()
        {
            if (!CanSendMessage())
                return;

            string messageText = MessageText.Trim();
            try
            {
                IsSending = true;
                MessageText = string.Empty; // Очищаем поле сразу

                Log.Info($"Sending message to chat {_chat.User.Id}: {messageText}");

                long result = await _networkService.SendMessageAsync(_chat.User.Id, messageText);

                if (result == default)
                {
                    MessageBox.Show("Не смог присвоить ID сообщению", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    MessageText = messageText;
                    Log.Error("SendMessageAsync failed to get ID of sended message");
                    return;
                }
                var message = new MessageModel(
                        result,
                        messageText,
                        DateTime.Now,
                        _networkService.CurrentUser
                    );

                OutgoingMessageSended?.Invoke(message, _chat.User.Id);
                App.Current.Dispatcher.Invoke(() =>
                {
                    _chat.Messages.Add(message);
                });

                Log.Success($"Message sent successfully: {message}");
            }
            catch (Exception ex)
            {
                Log.Error($"SendMessageAsync failed: {ex.Message}");
                MessageText = messageText;
            }
            finally
            {
                IsSending = false;
            }
        }
        private bool CanSendMessage()
        {
            return !IsSending && !string.IsNullOrWhiteSpace(MessageText.Trim());
        }

        /// <summary>
        /// Добавляет новое входящее сообщение
        /// </summary>
        public void AddIncomingMessage(MessageModel message)
        {
            //if (_chat.Messages.Any(m => m.Id == message.Id))
            //{
            //    Log.Warn($"Message {message.Id} already exists in chat {_chat.User.Id}, skipping.");
            //    return;
            //}
            //App.Current.Dispatcher.Invoke(() =>
            //{
            //    _chat.Messages.Add(message);
            //    Log.Info($"Added incoming message to chat {_chat.User.Id}");
            //});
        }

        #endregion

        #region Event Handlers

        private void ChatOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // Простая прокси-реакция: уведомляем view о соответствующих свойствах
            switch (e.PropertyName)
            {
                case nameof(ChatModel.User.Fullname):
                    RaisePropertyChanged(nameof(Title));
                    break;
                case nameof(ChatModel.User.Avatar):
                    RaisePropertyChanged(nameof(Avatar));
                    break;
                case nameof(ChatModel.UnreadCount):
                    RaisePropertyChanged(nameof(UnreadCount));
                    break;
                case nameof(ChatModel.User.IsOnline):
                    RaisePropertyChanged(nameof(IsOnline));
                    break;
                    // и т.д.
            }
        }

        #endregion


        public void Dispose()
        {
            if (_disposed) return;
            _chat.PropertyChanged -= ChatOnPropertyChanged;
            _disposed = true;
        }

        public event Action<MessageModel, long>? OutgoingMessageSended;

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

}
