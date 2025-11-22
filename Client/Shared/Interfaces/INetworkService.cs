using Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Shared.Interfaces;

public interface INetworkService
{
    /// <summary>
    /// Подключается к серверу.
    /// </summary>
    /// <param name="host">IP или имя хоста сервера</param>
    /// <param name="port">Порт для подключения</param>
    /// <returns>Успешно ли подключение</returns>
    Task<bool> ConnectAsync(string host, int port);
    /// <summary>
    /// Авторизует пользователя по токену.
    /// </summary>
    /// <param name="token"></param>
    /// <returns>ID авторизованного пользователя</returns>
    Task<long> AuthorizeAsync(string token);
    Task<(long userId, string token)> LoginAsync(string username, string password);
    Task<(long userId, string token)> RegisterAsync(string username, string password);
    Task<long> SendMessageAsync(long recipient, string text);
    Task<MessageModel[]> GetHistoryAsync(long userId);
    Task<UserModel[]> GetUserListAsync();
    Task PingAsync();
    Task<UserModel[]> SearchUsersAsync(string query);
    Task LogoutAsync();

    event Action<MessageModel> MessageReceived;
    event Action<(long userId, bool isOnline)> UserStatusChanged;
}
