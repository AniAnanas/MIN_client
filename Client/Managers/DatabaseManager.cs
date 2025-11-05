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
                INSERT INTO chats (id, title, avatar, is_online, unread_count, created_at, updated_at)
                VALUES (@id, @title, @avatar, @is_online, @unread_count, @created_at, @updated_at)
                ON CONFLICT (id) DO UPDATE SET
                    title = EXCLUDED.title,
                    avatar = EXCLUDED.avatar,
                    is_online = EXCLUDED.is_online,
                    unread_count = EXCLUDED.unread_count,
                    updated_at = EXCLUDED.updated_at";

            var parameters = new[]
            {
                new NpgsqlParameter("@id", chat.Id),
                new NpgsqlParameter("@title", chat.User.Username),
                new NpgsqlParameter("@avatar", chat.User.Avatar ?? (object)DBNull.Value),
                new NpgsqlParameter("@is_online", chat.User.IsOnline),
                new NpgsqlParameter("@unread_count", chat.UnreadCount),
                new NpgsqlParameter("@created_at", DateTime.UtcNow),
                new NpgsqlParameter("@updated_at", DateTime.UtcNow)
            };

            await ExecuteNonQueryAsync(query, parameters);
            _logger.Info($"Saved chat: {chat.User.Username}");
        }

        public async Task<ChatModel?> GetChatAsync(string id)
        {
            var query = "SELECT * FROM chats WHERE id = @id";
            var parameters = new[] { new NpgsqlParameter("@id", id) };

            return await ExecuteQueryAsync(query, reader => new ChatModel
                (
                    reader.GetInt32(0), // Fixed: Changed GetString to GetInt32 to match the 'int' type of Id
                    new UserModel(
                        id: reader.GetInt32(1), // Assuming User.Id is stored in column 1
                        username: reader.GetString(2), // Assuming User.Username is stored in column 2
                        name: reader.IsDBNull(3) ? "Unknown" : reader.GetString(3), // Assuming User.Name is stored in column 3
                        avatar: reader.IsDBNull(4) ? null : reader.GetString(4), // Assuming User.Avatar is stored in column 4
                        lastName: reader.IsDBNull(5) ? string.Empty : reader.GetString(5) // Assuming User.LastName is stored in column 5
                    )
                ) { UnreadCount = reader.GetInt32(6) } // Assuming UnreadCount is stored in column 6
            );
        }

        public async Task<List<ChatModel>> GetAllChatsAsync()
        {
            var query = "SELECT * FROM chats ORDER BY updated_at DESC";

            return await ExecuteQueryListAsync(query, reader => new ChatModel
                (
                    reader.GetInt32(0), // Fixed: Changed GetString to GetInt32 to match the 'int' type of Id
                    new UserModel(
                        id: reader.GetInt32(1), // Assuming User.Id is stored in column 1
                        username: reader.GetString(2), // Assuming User.Username is stored in column 2
                        name: reader.IsDBNull(3) ? "Unknown" : reader.GetString(3), // Assuming User.Name is stored in column 3
                        avatar: reader.IsDBNull(4) ? null : reader.GetString(4), // Assuming User.Avatar is stored in column 4
                        lastName: reader.IsDBNull(5) ? string.Empty : reader.GetString(5) // Assuming User.LastName is stored in column 5
                    )
                )
                { UnreadCount = reader.GetInt32(6) } // Assuming UnreadCount is stored in column 6
            );
        }

        public async Task SaveMessageAsync(MessageModel message)
        {
            var query = @"
                INSERT INTO messages (id, chat_id, sender_id, text, timestamp, is_own)
                VALUES (@id, @chat_id, @sender_id, @text, @timestamp, @is_own)";

            var parameters = new[]
            {
                new NpgsqlParameter("@id", message.Id),
                new NpgsqlParameter("@chat_id", string.Empty),
                new NpgsqlParameter("@sender_id", message.User.Id),
                new NpgsqlParameter("@text", message.Text),
                new NpgsqlParameter("@timestamp", message.Timestamp),
                new NpgsqlParameter("@is_own", message.IsOwn)
            };

            await ExecuteNonQueryAsync(query, parameters);
            _logger.Info($"Saved message: {message.Id}");
        }

        //public async Task<List<MessageModel>> GetChatMessagesAsync(string chatId, int offset = 0, int limit = 50)
        //{
        //    var query = "SELECT * FROM messages WHERE chat_id = @chat_id ORDER BY timestamp DESC LIMIT @limit OFFSET @offset";
        //    var parameters = new[]
        //    {
        //        new NpgsqlParameter("@chat_id", chatId),
        //        new NpgsqlParameter("@limit", limit),
        //        new NpgsqlParameter("@offset", offset)
        //    };

        //    return await ExecuteQueryListAsync(query, reader => new MessageModel
        //    {
        //        Id = reader.GetInt32(0),
        //        Chat = new ChatModel { Id = reader.GetString(1), Type = ChatType.Personal, Title = "User" },
        //        User = new UserModel(reader.GetInt32(2), "User"), // TODO: Load full user data
        //        Text = reader.GetString(3),
        //        Timestamp = reader.GetDateTime(4),
        //        IsOwn = reader.GetBoolean(5),
        //        Avatar = reader.IsDBNull(6) ? null : reader.GetString(6)
        //    });
        //}
    }
}
