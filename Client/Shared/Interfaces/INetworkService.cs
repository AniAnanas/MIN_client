using Client.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Shared.Interfaces;

public interface INetworkService
{
    Task<bool> ConnectAsync(string host, int port);
    Task<bool> AuthorizeAsync(string token);
    Task<string> LoginAsync(string username, string password);
    Task<string> RegisterAsync(string username, string password);
    Task<bool> SendMessageAsync(string sender, string recipient, string text);
    Task<string[]> GetUsersAsync();
    Task<string[]> GetPeersAsync();
    Task<MessageModel[]> GetHistoryAsync(string user);
    event Action<MessageModel> MessageReceived;
    event Action<string[]> UsersReceived;
}
