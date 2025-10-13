using Client.Services.Interfaces;
using Client.Models;
using System.Data;
using Npgsql;
using System.Text.Json;
namespace Client.Managers
{
    public class DatabaseManager
    {
        private readonly string _connectionString;
        private readonly ILoggingService _logger;

        public DatabaseManager(ILoggingService logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // TODO: Load from configuration
            _connectionString = "Host=localhost;Database=messenger;Username=postgres;Password=password";
        }

        public async Task<T?> ExecuteQueryAsync<T>(string query, Func<NpgsqlDataReader, T> mapper, params NpgsqlParameter[] parameters)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                using var reader = await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    return mapper(reader);
                }

                return default;
            }
            catch (Exception ex)
            {
                _logger.Error($"Database query error: {ex.Message}");
                return default;
            }
        }

        public async Task<List<T>> ExecuteQueryListAsync<T>(string query, Func<NpgsqlDataReader, T> mapper, params NpgsqlParameter[] parameters)
        {
            var results = new List<T>();

            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(mapper(reader));
                }

                return results;
            }
            catch (Exception ex)
            {
                _logger.Error($"Database query list error: {ex.Message}");
                return results;
            }
        }

        public async Task<int> ExecuteNonQueryAsync(string query, params NpgsqlParameter[] parameters)
        {
            try
            {
                using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddRange(parameters);

                return await command.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                _logger.Error($"Database non-query error: {ex.Message}");
                return 0;
            }
        }

        public async Task SaveChatAsync(ChatModel chat)
        {
            var query = @"
                INSERT INTO chats (id, title, avatar, type, is_online, unread_count, created_at, updated_at)
                VALUES (@id, @title, @avatar, @type, @is_online, @unread_count, @created_at, @updated_at)
                ON CONFLICT (id) DO UPDATE SET
                    title = EXCLUDED.title,
                    avatar = EXCLUDED.avatar,
                    type = EXCLUDED.type,
                    is_online = EXCLUDED.is_online,
                    unread_count = EXCLUDED.unread_count,
                    updated_at = EXCLUDED.updated_at";

            var parameters = new[]
            {
                new NpgsqlParameter("@id", chat.Id),
                new NpgsqlParameter("@title", chat.Title),
                new NpgsqlParameter("@avatar", chat.Avatar ?? (object)DBNull.Value),
                new NpgsqlParameter("@type", (int)chat.Type),
                new NpgsqlParameter("@is_online", chat.IsOnline),
                new NpgsqlParameter("@unread_count", chat.UnreadCount),
                new NpgsqlParameter("@created_at", DateTime.UtcNow),
                new NpgsqlParameter("@updated_at", DateTime.UtcNow)
            };

            await ExecuteNonQueryAsync(query, parameters);
            _logger.Info($"Saved chat: {chat.Title}");
        }

        public async Task<ChatModel?> GetChatAsync(string id)
        {
            var query = "SELECT * FROM chats WHERE id = @id";
            var parameters = new[] { new NpgsqlParameter("@id", id) };

            return await ExecuteQueryAsync(query, reader => new ChatModel
            {
                Id = reader.GetString(0),
                Title = reader.GetString(1),
                Avatar = reader.IsDBNull(2) ? null : reader.GetString(2),
                Type = (ChatType)reader.GetInt32(3),
                IsOnline = reader.GetBoolean(4),
                UnreadCount = reader.GetInt32(5)
            }, parameters);
        }

        public async Task<List<ChatModel>> GetAllChatsAsync()
        {
            var query = "SELECT * FROM chats ORDER BY updated_at DESC";

            return await ExecuteQueryListAsync(query, reader => new ChatModel
            {
                Id = reader.GetString(0),
                Title = reader.GetString(1),
                Avatar = reader.IsDBNull(2) ? null : reader.GetString(2),
                Type = (ChatType)reader.GetInt32(3),
                IsOnline = reader.GetBoolean(4),
                UnreadCount = reader.GetInt32(5)
            });
        }

        public async Task SaveMessageAsync(MessageModel message)
        {
            var query = @"
                INSERT INTO messages (id, chat_id, sender_id, text, timestamp, is_own, avatar)
                VALUES (@id, @chat_id, @sender_id, @text, @timestamp, @is_own, @avatar)";

            var parameters = new[]
            {
                new NpgsqlParameter("@id", message.Id),
                new NpgsqlParameter("@chat_id", message.Chat.Id ?? string.Empty),
                new NpgsqlParameter("@sender_id", message.Sender.Id),
                new NpgsqlParameter("@text", message.Text),
                new NpgsqlParameter("@timestamp", message.Timestamp),
                new NpgsqlParameter("@is_own", message.IsOwn),
                new NpgsqlParameter("@avatar", message.Avatar ?? (object)DBNull.Value)
            };

            await ExecuteNonQueryAsync(query, parameters);
            _logger.Info($"Saved message: {message.Id}");
        }

        public async Task<List<MessageModel>> GetChatMessagesAsync(string chatId, int offset = 0, int limit = 50)
        {
            var query = "SELECT * FROM messages WHERE chat_id = @chat_id ORDER BY timestamp DESC LIMIT @limit OFFSET @offset";
            var parameters = new[]
            {
                new NpgsqlParameter("@chat_id", chatId),
                new NpgsqlParameter("@limit", limit),
                new NpgsqlParameter("@offset", offset)
            };

            return await ExecuteQueryListAsync(query, reader => new MessageModel
            {
                Id = reader.GetInt32(0),
                Chat = new ChatModel { Id = reader.GetString(1), Type = ChatType.Personal, Title = "User" },
                Sender = new UserModel(reader.GetInt32(2), "User"), // TODO: Load full user data
                Text = reader.GetString(3),
                Timestamp = reader.GetDateTime(4),
                IsOwn = reader.GetBoolean(5),
                Avatar = reader.IsDBNull(6) ? null : reader.GetString(6)
            });
        }
    }
}
