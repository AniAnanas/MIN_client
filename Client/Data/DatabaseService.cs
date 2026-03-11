using Client.Models;
using Client.Shared.Extensions;
using Client.Shared.Helpers.DB;
using Client.Shared.Interfaces;
using Microsoft.Data.Sqlite;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Client.Data;

public class DatabaseService : IDisposable
{
    private IDbConnection _connection;
    private IQueryBuilder _queryBuilder;
    private SqlTableCreator _tableCreator;
    private bool _isInitialized;
    private long _currentUserId = -1;


    private SqlTable UsersTable => new SqlTable("Users",
        new SqlColumn("Id", MySqlDbType.Int64) { Primary = true, NotNull = true },
        new SqlColumn("Username", MySqlDbType.VarChar, 50) { NotNull = true },
        new SqlColumn("DisplayName", MySqlDbType.VarChar, 100),
        new SqlColumn("IsOnline", MySqlDbType.Int32) { NotNull = true, DefaultValue = "0" },
        new SqlColumn("LastSeen", MySqlDbType.DateTime),
        new SqlColumn("UpdatedAt", MySqlDbType.DateTime) { DefaultCurrentTimestamp = true }
    );

    private SqlTable MessagesTable => new SqlTable("Messages",
        new SqlColumn("Id", MySqlDbType.Int64) { Primary = true, NotNull = true },
        new SqlColumn("ChatId", MySqlDbType.Int64) { NotNull = true }, // ID собеседника
        new SqlColumn("SenderId", MySqlDbType.Int64) { NotNull = true }, // ID отправителя (мы или собеседник)
        new SqlColumn("IsOutgoing", MySqlDbType.Int32) { NotNull = true }, // 1 = мы отправили, 0 = получили
        new SqlColumn("Text", MySqlDbType.Text) { NotNull = true },
        new SqlColumn("Timestamp", MySqlDbType.Int64) { NotNull = true },
        new SqlColumn("IsRead", MySqlDbType.Int32) { NotNull = true, DefaultValue = "0" },
        new SqlColumn("CreatedAt", MySqlDbType.DateTime) { DefaultCurrentTimestamp = true }
    );

    private SqlTable ChatsTable => new SqlTable("Chats",
        new SqlColumn("ChatId", MySqlDbType.Int64) { Primary = true, NotNull = true }, // ID собеседника
        new SqlColumn("LastMessageId", MySqlDbType.Int64),
        new SqlColumn("LastMessageText", MySqlDbType.Text),
        new SqlColumn("LastMessageTime", MySqlDbType.Int64),
        new SqlColumn("UnreadCount", MySqlDbType.Int32) { NotNull = true, DefaultValue = "0" },
        new SqlColumn("UpdatedAt", MySqlDbType.DateTime) { DefaultCurrentTimestamp = true }
    );

    private SqlTable SettingsTable => new SqlTable("Settings",
        new SqlColumn("Key", MySqlDbType.VarChar, 100) { Primary = true, NotNull = true },
        new SqlColumn("Value", MySqlDbType.Text)
    );

    public DatabaseService(string databasePath = "Data/MIN.sqlite")
    {
        try
        {
            Log.Info($"Database initializing...");

            string connectionString = $"Data Source={databasePath};";
            _connection = new SqliteConnection(connectionString);
            _connection.Open();

            _queryBuilder = new SqliteQueryCreator();
            _tableCreator = new SqlTableCreator(_connection, _queryBuilder);

            InitializeTables();
            _isInitialized = true;
            Log.Info($"Database initialized: {databasePath}");
        }
        catch (Exception ex)
        {
            Log.Error($"Database initialization failed: {ex.Message}");
            throw;
        }
    }

    private void InitializeTables()
    {
        _tableCreator.EnsureTableStructure(UsersTable);
        _tableCreator.EnsureTableStructure(MessagesTable);
        _tableCreator.EnsureTableStructure(ChatsTable);
        _tableCreator.EnsureTableStructure(SettingsTable);

        // Создаём индекс для быстрого поиска сообщений по чату
        try
        {
            _connection.Query("CREATE INDEX IF NOT EXISTS idx_messages_chat ON 'Messages'(ChatId, Timestamp DESC)");
            _connection.Query("CREATE INDEX IF NOT EXISTS idx_messages_sender ON 'Messages'(SenderId)");
        }
        catch { /* Индекс уже существует */ }

        Log.Info("Database tables ensured");
    }

    #region Current User

    public void SetCurrentUser(long userId)
    {
        _currentUserId = userId;
        SaveSetting("CurrentUserId", userId.ToString());
        Log.Info($"Current user set: {userId}");
    }

    public long GetCurrentUserId()
    {
        if (_currentUserId == -1)
        {
            var saved = GetSetting("CurrentUserId");
            if (long.TryParse(saved, out var userId))
            {
                _currentUserId = userId;
            }
        }
        return _currentUserId;
    }

    #endregion

    #region Users Operations

    public void SaveOrUpdateUser(long userId, string username, string displayName = null, bool? isOnline = null)
    {
        try
        {
            // Проверяем, существует ли пользователь
            var query = $"SELECT COUNT(*) AS total FROM 'Users' WHERE Id = {userId}";
            bool exists = false;
            using (var reader = _connection.QueryReader(query))
            {
                if (reader.Read())
                    exists = reader.Get<long>("total") > 0;

            }

            if (exists)
            {
                // Обновляем
                var wheres = new List<SqlValue> { new SqlValue("Id", userId) };
                var values = new List<SqlValue>
                {
                    new SqlValue("Username", username),
                    new SqlValue("UpdatedAt", DateTime.Now.Ticks / 10000)
                };

                if (!string.IsNullOrWhiteSpace(displayName))
                    values.Add(new SqlValue("DisplayName", displayName));

                if (isOnline.HasValue)
                {
                    values.Add(new SqlValue("IsOnline", isOnline.Value ? 1 : 0));
                    if (!isOnline.Value)
                        values.Add(new SqlValue("LastSeen", DateTime.Now.Ticks / 10000));
                }

                _connection.Query(_queryBuilder.UpdateValue("Users", values, wheres));
            }
            else
            {
                // Создаём нового
                var values = new List<SqlValue>
                {
                    new SqlValue("Id", userId),
                    new SqlValue("Username", username),
                    new SqlValue("DisplayName", displayName ?? username),
                    new SqlValue("IsOnline", isOnline ?? false ? 1 : 0)
                };

                _connection.Query(_queryBuilder.InsertValues("Users", values));
            }
        }
        catch (Exception ex)
        {
            Log.Error($"SaveOrUpdateUser failed: {ex.Message}");
        }
    }

    public UserModel GetUserById(long userId)
    {
        try
        {
            var wheres = new List<SqlValue> { new SqlValue("Id", userId) };
            using var reader = _connection.QueryReader(_queryBuilder.ReadColumn("Users", wheres));

            if (reader.Read())
            {
                long id = reader.Get<long>("Id");
                string username = reader.Get<string>("Username");
                string name = reader.Get<string>("DisplayName");
                var result = new UserModel(id, username, (name.IsNullOrSpace() ? username : name))
                {
                    IsOnline = reader.Get<int>("IsOnline") == 1
                };
                Log.Info(result.ToString());
                return result;
            }
        }
        catch (Exception ex)
        {
            Log.Error($"GetUserById failed: {ex.Message}");
        }

        return null;
    }

    public List<UserModel> GetAllUsers()
    {
        var users = new List<UserModel>();

        try
        {
            using (var reader = _connection.QueryReader("SELECT * FROM 'Users'"))
            {
                while (reader.Read())
                {
                    users.Add(new UserModel(
                        reader.Get<long>("Id"),
                        reader.Get<string>("Username"),
                        reader.Get<string>("DisplayName") ?? reader.Get<string>("Username")
                    )
                    {
                        IsOnline = reader.Get<int>("IsOnline") == 1
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"GetAllUsers failed: {ex.Message}");
        }

        return users;
    }

    public void UpdateUserOnlineStatus(long userId, bool isOnline)
    {
        try
        {
            var wheres = new List<SqlValue> { new SqlValue("Id", userId) };
            var values = new List<SqlValue>
            {
                new SqlValue("IsOnline", isOnline ? 1 : 0),
                new SqlValue("UpdatedAt", DateTime.Now.Ticks / 10000)
            };

            if (!isOnline)
                values.Add(new SqlValue("LastSeen", DateTime.Now));

            _connection.Query(_queryBuilder.UpdateValue("Users", values, wheres));
        }
        catch (Exception ex)
        {
            Log.Error($"UpdateUserOnlineStatus failed: {ex.Message}");
        }
    }

    #endregion

    #region Messages Operations

    public void SaveMessage(long messageId, long chatId, long senderId, string text, long timestamp, bool isOutgoing)
    {
        try
        {
            var values = new List<SqlValue>
            {
                new SqlValue("Id", messageId),
                new SqlValue("ChatId", chatId), // ID собеседника
                new SqlValue("SenderId", senderId), // Кто отправил
                new SqlValue("IsOutgoing", isOutgoing ? 1 : 0), // Мы или собеседник
                new SqlValue("Text", text),
                new SqlValue("Timestamp", timestamp),
                new SqlValue("IsRead", isOutgoing ? 1 : 0) // Исходящие сразу "прочитаны"
            };

            _connection.Query(_queryBuilder.InsertValues("Messages", values));

            // Обновляем информацию о чате
            UpdateChatInfo(chatId, messageId, text, timestamp, isOutgoing ? 0 : 1);

            Log.Info($"Message saved: {messageId}, chat: {chatId}, outgoing: {isOutgoing}");
        }
        catch (Exception ex)
        {
            Log.Error($"SaveMessage failed: {ex.Message}");
        }
    }

    public void SaveMessageFromModel(MessageModel message, long chatId, bool isOutgoing)
    {
        var timestamp = new DateTimeOffset(message.Timestamp).ToUnixTimeSeconds();
        SaveMessage(message.Id, chatId, message.User.Id, message.Text, timestamp, isOutgoing);
    }

    public List<MessageModel> GetChatHistory(long chatId, int limit = 50, long? beforeMessageId = null)
    {
        var messages = new List<MessageModel>();

        try
        {
            string query;
            if (beforeMessageId.HasValue)
            {
                query = $"SELECT * FROM 'Messages' WHERE ChatId = {chatId} AND Id < {beforeMessageId.Value} ORDER BY Timestamp DESC LIMIT {limit}";
            }
            else
            {
                query = $"SELECT * FROM 'Messages' WHERE ChatId = {chatId} ORDER BY Timestamp DESC LIMIT {limit}";
            }

            using var reader = _connection.QueryReader(query);

            while (reader.Read())
            {
                var senderId = reader.Get<long>("SenderId");
                var isOutgoing = reader.Get<int>("IsOutgoing") == 1;

                messages.Add(new MessageModel(
                    reader.Get<long>("Id"),
                    reader.Get<string>("Text"),
                    DateTimeOffset.FromUnixTimeSeconds(reader.Get<long>("Timestamp")).LocalDateTime,
                    GetUserById(senderId) ?? new UserModel(senderId, "Unknown", "Unknown")
                ));
            }

            // Разворачиваем, т.к. получили в обратном порядке
            messages.Reverse();
        }
        catch (Exception ex)
        {
            Log.Error($"GetChatHistory failed: {ex.Message}");
        }

        return messages;
    }

    public void MarkChatAsRead(long chatId)
    {
        try
        {
            // Помечаем все входящие сообщения в чате как прочитанные
            var query = $"UPDATE 'Messages' SET IsRead = 1 WHERE ChatId = {chatId} AND IsOutgoing = 0 AND IsRead = 0"; // `AND IsOutgoing = 0 ` - can be deleted to support unread
                                                                                                                       // state of "sended planned messages" like in tg
            _connection.Query(query);

            // Сбрасываем счётчик непрочитанных
            var wheres = new List<SqlValue> { new SqlValue("ChatId", chatId) };
            var values = new List<SqlValue> { new SqlValue("UnreadCount", 0) };
            _connection.Query(_queryBuilder.UpdateValue("Chats", values, wheres));

            Log.Info($"Chat {chatId} marked as read");
        }
        catch (Exception ex)
        {
            Log.Error($"MarkChatAsRead failed: {ex.Message}");
        }
    }

    public int GetUnreadCount(long chatId)
    {
        try
        {
            var wheres = new List<SqlValue> { new SqlValue("ChatId", chatId) };
            using var reader = _connection.QueryReader(_queryBuilder.ReadColumn("Chats", wheres));

            if (reader.Read())
            {
                return reader.Get<int>("UnreadCount");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"GetUnreadCount failed: {ex.Message}");
        }

        return 0;
    }

    #endregion

    #region Chats Operations

    private void UpdateChatInfo(long chatId, long lastMessageId, string lastMessageText, long lastMessageTime, int incrementUnread)
    {
        try
        {
            // Проверяем, существует ли чат
            var query = $"SELECT UnreadCount FROM 'Chats' WHERE ChatId = {chatId}";

            bool exist = false;
            int currentUnread = 0;

            using (var reader = _connection.QueryReader(query))
            {
                if (reader.Read())
                {
                    exist = true;
                    currentUnread = reader.Get<int>("UnreadCount");
                }
            }
                
            if (exist)
            {
                var wheres = new List<SqlValue> { new("ChatId", chatId) };
                var values = new List<SqlValue>
                {
                    new("LastMessageId", lastMessageId),
                    new("LastMessageText", lastMessageText.Length > 100 ? lastMessageText[..100] : lastMessageText),
                    new("LastMessageTime", lastMessageTime),
                    new("UnreadCount", currentUnread + incrementUnread),
                    new("UpdatedAt", DateTime.Now.Ticks / 10000)
                };
                _connection.Query(_queryBuilder.UpdateValue("Chats", values, wheres));
            }
            else
            {
                var values = new List<SqlValue>
                {
                    new("ChatId", chatId),
                    new("LastMessageId", lastMessageId),
                    new("LastMessageText", lastMessageText.Length > 100 ? lastMessageText[..100] : lastMessageText),
                    new("LastMessageTime", lastMessageTime),
                    new("UnreadCount", incrementUnread)
                };
                _connection.Query(_queryBuilder.InsertValues("Chats", values));
            }

        }
        catch (Exception ex)
        {
            Log.Error($"UpdateChatInfo failed: {ex.Message}");
        }
    }

    public List<(long chatId, UserModel user, string lastMessage, DateTime lastMessageTime, int unreadCount)> GetAllChats()
    {
        var chats = new List<(long, UserModel, string, DateTime, int)>();

        try
        {
            var query = "SELECT * FROM 'Chats' ORDER BY LastMessageTime DESC";
            using var reader = _connection.QueryReader(query);

            while (reader.Read())
            {
                var chatId = reader.Get<long>("ChatId");
                var user = GetUserById(chatId);

                if (user != null)
                {
                    chats.Add((
                        chatId,
                        user,
                        reader.Get<string>("LastMessageText") ?? "",
                        DateTimeOffset.FromUnixTimeSeconds(reader.Get<long>("LastMessageTime")).LocalDateTime,
                        reader.Get<int>("UnreadCount")
                    ));
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"GetAllChats failed: {ex.Message}");
        }

        return chats;
    }

    public void DeleteChat(long chatId)
    {
        try
        {
            _connection.Query($"DELETE FROM 'Messages' WHERE ChatId = {chatId}");
            _connection.Query($"DELETE FROM 'Chats' WHERE ChatId = {chatId}");
            Log.Info($"Chat {chatId} deleted");
        }
        catch (Exception ex)
        {
            Log.Error($"DeleteChat failed: {ex.Message}");
        }
    }

    #endregion

    #region Settings Operations

    public void SaveSetting(string key, string value)
    {
        try
        {
            var wheres = new List<SqlValue> { new SqlValue("Key", key) };
            var values = new List<SqlValue> { new SqlValue("Value", value) };

            var affected = _connection.Query(_queryBuilder.UpdateValue("Settings", values, wheres));

            if (affected == 0)
            {
                values.Add(new SqlValue("Key", key));
                _connection.Query(_queryBuilder.InsertValues("Settings", values));
            }
        }
        catch (Exception ex)
        {
            Log.Error($"SaveSetting failed: {ex.Message}");
        }
    }

    public string GetSetting(string key, string defaultValue = null)
    {
        try
        {
            var wheres = new List<SqlValue> { new SqlValue("Key", key) };
            using var reader = _connection.QueryReader(_queryBuilder.ReadColumn("Settings", wheres));

            if (reader.Read())
            {
                return reader.Get<string>("Value");
            }
        }
        catch (Exception ex)
        {
            Log.Error($"GetSetting failed: {ex.Message}");
        }

        return defaultValue;
    }

    #endregion

    #region Utility Methods

    public void ClearAllData()
    {
        try
        {
            _connection.Query("DELETE FROM 'Messages'");
            _connection.Query("DELETE FROM 'Chats'");
            _connection.Query("DELETE FROM 'Users'");
            _connection.Query("DELETE FROM 'Settings' WHERE Key != 'CurrentUserId'");
            Log.Info("All data cleared");
        }
        catch (Exception ex)
        {
            Log.Error($"ClearAllData failed: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _connection?.Close();
        _connection?.Dispose();
        _isInitialized = false;
        Log.Info("Database connection closed");
    }

    #endregion
}
